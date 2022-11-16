using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Lucy.Core.SourceGenerator.Generators
{
    public class DbExtensionMethodGenerator
    {
        private static readonly string Ns = "Lucy.Core.SemanticAnalysis.Infrastructure";
        private static readonly Logger Logger = new Logger("extension", true);

        public static void Register(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource("HandlerExtension.g.cs", SourceText.From($$"""
                namespace {{Ns}}
                {
                    [System.AttributeUsage(System.AttributeTargets.Method)]
                    public class GenerateDbExtension : System.Attribute {}
                }
                """, Encoding.UTF8)));

            if (Logger.IsEnabled)
            {
                Logger.Write("Started...");
            }

            IncrementalValuesProvider<MethodDeclarationSyntax> classDeclarationFilter = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: (node, _) => node is MethodDeclarationSyntax,
                transform: (ctx, _) => GetMethodDeclarationIfMatches(ctx))
            .Where(m => m != null)!;

            IncrementalValueProvider<(Compilation, ImmutableArray<MethodDeclarationSyntax>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarationFilter.Collect());

            context.RegisterSourceOutput(compilationAndClasses, (sps, source) => BuildSource(sps, source.Item1, source.Item2));

        }

        static MethodDeclarationSyntax? GetMethodDeclarationIfMatches(GeneratorSyntaxContext context)
        {
            var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;
            foreach (var attributeListSyntax in methodDeclarationSyntax.AttributeLists)
            {
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is IMethodSymbol symbol &&
                        symbol.ContainingType.ToDisplayString() == Ns + ".GenerateDbExtension")
                    {
                        if (Logger.IsEnabled)
                        {
                            Logger.Write("Found " + methodDeclarationSyntax.Identifier.Text);
                        }
                        return methodDeclarationSyntax;
                    }
                }
            }

            return null;
        }

        private static void BuildSource(SourceProductionContext sps, Compilation compilation, ImmutableArray<MethodDeclarationSyntax> methods)
        {
            var sp = new StringBuilder();
            foreach (var method in methods)
            {
                sps.CancellationToken.ThrowIfCancellationRequested();

                var sm = compilation.GetSemanticModel(method.SyntaxTree);

                var methodInfo = sm.GetDeclaredSymbol(method);
                if (methodInfo == null)
                {
                    if (Logger.IsEnabled)
                        Logger.Write("Skipped " + method.Identifier.Text + " because the declared symbol could not be resolved.");
                    continue;
                }
                    

                var methodReturnTypeInfo = sm.GetSymbolInfo(method.ReturnType).Symbol as INamedTypeSymbol;
                if (methodReturnTypeInfo == null)
                {
                    if (Logger.IsEnabled)
                        Logger.Write("Skipped " + method.Identifier.Text + " because the return type symbol could not be resolved.");
                    continue;
                }
                    

                var @class = method.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                if (@class == null)
                {
                    if (Logger.IsEnabled)
                        Logger.Write("Skipped " + method.Identifier.Text + " because the class could not be resolved.");
                    continue;
                }
                    

                List<string> myArguments = new();
                List<string> handlerArguments = new();
                List<string> myMethodParameter = new();
                List<string> myRecordParameter = new();
                bool failed = false;
                foreach (var methodParameter in method.ParameterList.Parameters)
                {
                    if (methodParameter.Type == null)
                    {
                        if (Logger.IsEnabled)
                            Logger.Write("Skipped " + method.Identifier.Text + " because the parameter " + methodParameter.Identifier.Text + " type was not provided.");
                        failed= true;
                        break;
                    }
                        

                    var typeSymbol = sm.GetSymbolInfo(methodParameter.Type).Symbol as INamedTypeSymbol;
                    if (typeSymbol == null)
                    {
                        if (Logger.IsEnabled)
                            Logger.Write("Skipped " + method.Identifier.Text + " because the parameter " + methodParameter.Identifier.Text + " type symbol could not be resolved.");
                        failed = true;
                        break;
                    }
                        

                    if (typeSymbol.ToDisplayString() == Ns + ".IDb")
                        continue;

                    myArguments.Add(methodParameter.Identifier.Text);
                    myMethodParameter.Add(typeSymbol.ToDisplayString() + " " + methodParameter.Identifier.Text);
                    myRecordParameter.Add(typeSymbol.ToDisplayString() + " " + methodParameter.Identifier.Text);
                    handlerArguments.Add("query." + methodParameter.Identifier.Text);
                }

                if (failed)
                    continue;

                handlerArguments.Insert(0, "db");
                myMethodParameter.Insert(0, $"this {Ns}.IDb db");

                sp.AppendLine($$"""
                    namespace {{methodInfo.ContainingNamespace}}
                    {
                        public record {{method.Identifier.Text}}Input({{string.Join(", ", myRecordParameter)}}) : {{Ns}}.IQuery<{{method.Identifier.Text}}Output>;
                        public record {{method.Identifier.Text}}Output({{methodReturnTypeInfo.ToDisplayString()}} Result);

                        public static class {{method.Identifier.Text}}Ex
                        {
                            ///<summary>
                            ///Implementation: <see cref="{{@class.Identifier.Text}}.{{method.Identifier.Text}}" />
                            ///</summary>
                            public static {{methodReturnTypeInfo.ToDisplayString()}} {{method.Identifier.Text}}({{string.Join(", ", myMethodParameter)}})
                            {
                                return db.Query(new {{method.Identifier.Text}}Input({{string.Join(",", myArguments)}})).Result;
                            }
                        }

                        public class {{method.Identifier.Text}}GeneratedHandler : {{Ns}}.QueryHandler<{{method.Identifier.Text}}Input, {{method.Identifier.Text}}Output>
                        {
                            public override {{method.Identifier.Text}}Output Handle({{Ns}}.IDb db, {{method.Identifier.Text}}Input query)
                            {
                                return new {{method.Identifier.Text}}Output({{@class.Identifier.Text}}.{{method.Identifier.Text}}({{string.Join(", ", handlerArguments)}}));
                            }
                        }
                    }

                    """);
            }
            sps.AddSource("DbExtensionMethods.g.cs", SourceText.From(sp.ToString(), Encoding.UTF8));
        }

    }
}
