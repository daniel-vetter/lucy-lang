using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Lucy.Core.SourceGenerator
{
    [Generator]
    public class SyntaxTreeModelGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            CreateModel(context);
            CreateStuff(context);
        }

        private void CreateStuff(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource("HandlerExtension.g.cs", SourceText.From(@"
namespace DbHelper
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class DbQueryHandlerAttribute : System.Attribute {}
}
            ", Encoding.UTF8)));

            IncrementalValuesProvider<MethodDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is MethodDeclarationSyntax, // select enums with attributes
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [EnumExtensions] attribute
            .Where(static m => m is not null)!; // filter out attributed enums that we don't care about

            IncrementalValueProvider<(Compilation, ImmutableArray<MethodDeclarationSyntax>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses, (sps, source) => Execute(sps, source.Item1, source.Item2));

        }

        private void Execute(SourceProductionContext sps, Compilation compilation, ImmutableArray<MethodDeclarationSyntax> methods)
        {
            var r = new StringBuilder();
            foreach (var method in methods)
            {
                sps.CancellationToken.ThrowIfCancellationRequested();

                var sm = compilation.GetSemanticModel(method.SyntaxTree);

                var methodInfo = sm.GetDeclaredSymbol(method);
                if (methodInfo == null)
                    continue;

                var methodReturnTypeInfo = sm.GetSymbolInfo(method.ReturnType).Symbol as INamedTypeSymbol;
                if (methodReturnTypeInfo == null)
                    continue;

                var @class = method.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                if (@class == null)
                    continue;

                List<string> myArguments = new();
                List<string> handlerArguments = new();
                List<string> myMethodParameter = new();
                List<string> myRecordParameter = new();
                foreach (var methodParameter in method.ParameterList.Parameters)
                {
                    if (methodParameter.Type == null)
                        return;

                    var typeSymbol = sm.GetSymbolInfo(methodParameter.Type).Symbol as INamedTypeSymbol;
                    if (typeSymbol == null)
                        return;

                    if (typeSymbol.ToDisplayString() == "Lucy.Core.SemanticAnalysis.Infrasturcture.IDb")
                        continue;

                    myArguments.Add(methodParameter.Identifier.Text);
                    myMethodParameter.Add(typeSymbol.ToDisplayString() + " " + methodParameter.Identifier.Text);
                    myRecordParameter.Add(typeSymbol.ToDisplayString() + " " + methodParameter.Identifier.Text);
                    handlerArguments.Add("query." + methodParameter.Identifier.Text);
                }

                handlerArguments.Insert(0, "db");
                myMethodParameter.Insert(0, "this Lucy.Core.SemanticAnalysis.Infrasturcture.IDb db");

                r.AppendLine("namespace " + methodInfo.ContainingNamespace);
                r.AppendLine("{");
                r.AppendLine($"    public record {method.Identifier.Text}Input({string.Join(", ", myRecordParameter)}) : Lucy.Core.SemanticAnalysis.Infrasturcture.IQuery<{method.Identifier.Text}Output>;");
                r.AppendLine($"    public record {method.Identifier.Text}Output({methodReturnTypeInfo.ToDisplayString()} Result);");
                r.AppendLine();
                r.AppendLine("    public static class " + method.Identifier.Text + "Ex");
                r.AppendLine("    {");
                r.AppendLine("        /// <summary>");
                r.AppendLine($"        /// <see cref=\"{@class.Identifier.Text}.{method.Identifier.Text}\" />");
                r.AppendLine("        /// </summary>");
                r.AppendLine($"        public static {methodReturnTypeInfo.ToDisplayString()} " + method.Identifier.Text + "(" + string.Join(", ", myMethodParameter) + ")");
                r.AppendLine("        {");
                r.AppendLine($"            return db.Query(new {method.Identifier.Text}Input({string.Join(",", myArguments)})).Result;");
                r.AppendLine("        }");
                r.AppendLine("    }");
                r.AppendLine();
                r.AppendLine($"    public class {method.Identifier.Text}GeneratedHandler : Lucy.Core.SemanticAnalysis.Infrasturcture.QueryHandler<{method.Identifier.Text}Input, {method.Identifier.Text}Output>");
                r.AppendLine("    {");
                r.AppendLine($"        public override {method.Identifier.Text}Output Handle(Lucy.Core.SemanticAnalysis.Infrasturcture.IDb db, {method.Identifier.Text}Input query)");
                r.AppendLine("        {");
                r.AppendLine($"            return new {method.Identifier.Text}Output({@class.Identifier.Text}.{method.Identifier.Text}({string.Join(", ", handlerArguments)}));");
                r.AppendLine("        }");
                r.AppendLine("    }");
                r.AppendLine("}");



            }
            sps.AddSource("handlerExtension.g.cs", SourceText.From(r.ToString(), Encoding.UTF8));
        }

        static MethodDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
            var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;

            // loop through all the attributes on the method
            foreach (AttributeListSyntax attributeListSyntax in methodDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    {
                        // weird, we couldn't get the symbol, ignore it
                        continue;
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    string fullName = attributeContainingTypeSymbol.ToDisplayString();

                    // Is the attribute the [EnumExtensions] attribute?
                    if (fullName == "DbHelper.DbQueryHandlerAttribute")
                    {
                        // return the enum
                        return methodDeclarationSyntax;
                    }
                }
            }

            // we didn't find the attribute we were looking for
            return null;
        }

        private static void CreateModel(IncrementalGeneratorInitializationContext context)
        {
            var files = context.AdditionalTextsProvider
                            .Where(a => a.Path.EndsWith("model.xml"))
                            .Select((a, c) => (name: Path.GetFileNameWithoutExtension(a.Path), content: a.GetText(c)!.ToString()));

            var compilationAndFiles = context.CompilationProvider.Combine(files.Collect());

            context.RegisterSourceOutput(compilationAndFiles, (productionContext, sourceContext) =>
            {

                foreach (var (name, configXml) in sourceContext.Right)
                {
                    var config = ConfigLoader.GetConfig(configXml);

                    ModelGenerator.Generate(productionContext, name, config);
                    //FlatModelGenerator.Generate(productionContext, name, config);
                    ImmutableModelGenerator.Generate(productionContext, name, config);
                }
            });
        }
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
