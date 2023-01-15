﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Lucy.Core.SourceGenerator.Generators;

public static class DbExtensionMethodGenerator
{
    private const string _ns = "Lucy.Core.SemanticAnalysis.Infrastructure";
    private static readonly Logger _logger = new("extension", true);

    public static void Register(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("HandlerExtension.g.cs", SourceText.From($$"""
                // <auto-generated/>
                #nullable enable
                namespace {{_ns}}
                {
                    [System.AttributeUsage(System.AttributeTargets.Method)]
                    public class DbQuery : System.Attribute 
                    {
                        public DbQuery() {}
                        public DbQuery(bool cached) {}
                        public DbQuery(bool cached, string name) {}
                    }

                    [System.AttributeUsage(System.AttributeTargets.Method)]
                    public class DbExtension : System.Attribute {}

                    [System.AttributeUsage(System.AttributeTargets.Interface)]
                    public class DbInputs : System.Attribute {}
                }
                """, Encoding.UTF8)));

        if (_logger.IsEnabled)
        {
            _logger.Write("Started...");
        }

        var dbQueryMethodFilter = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is MethodDeclarationSyntax,
                transform: static (ctx, _) => GetDbQueryMethodDeclarationIfMatches(ctx))
            .Where(static m => m != null)
            .Select(static (x, _) => x!)
            .Collect();

        var inputInterfaceFilter = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is InterfaceDeclarationSyntax,
                transform: static (ctx, _) => GetInterfaceDeclarationIfMatches(ctx))
            .Where(static m => m != null)
            .Select(static (x, _) => x!)
            .Collect();

        var compilationAndMethods
            = context.CompilationProvider.Combine(dbQueryMethodFilter);

        var compilationAndInputInterfaces
            = context.CompilationProvider.Combine(inputInterfaceFilter);

        context.RegisterSourceOutput(compilationAndMethods, BuildMethodSource);
        context.RegisterSourceOutput(compilationAndInputInterfaces, BuildInputInterfaceSource);
    }

    private static MethodDeclarationSyntax? GetDbQueryMethodDeclarationIfMatches(GeneratorSyntaxContext context)
    {
        try
        {
            var methodDeclarationSyntax = (MethodDeclarationSyntax)context.Node;
            foreach (var attributeListSyntax in methodDeclarationSyntax.AttributeLists)
            {
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is IMethodSymbol symbol &&
                        symbol.ContainingType.ToDisplayString() == _ns + ".DbQuery")
                    {
                        return methodDeclarationSyntax;
                    }
                }
            }
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled)
                _logger.Write("Filter crashed: " + e);
        }

        return null;
    }

    private static InterfaceDeclarationSyntax? GetInterfaceDeclarationIfMatches(GeneratorSyntaxContext context)
    {
        try
        {
            var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;
            foreach (var attributeListSyntax in interfaceDeclarationSyntax.AttributeLists)
            {
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is IMethodSymbol symbol &&
                        symbol.ContainingType.ToDisplayString() == _ns + ".DbInputs")
                    {
                        return interfaceDeclarationSyntax;
                    }
                }
            }
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled)
                _logger.Write("Filter crashed: " + e);
        }

        return null;
    }

    private static void BuildInputInterfaceSource(SourceProductionContext sps, (Compilation Compilation, ImmutableArray<InterfaceDeclarationSyntax> Interfaces) source)
    {
        try
        {
            foreach (var @interface in source.Interfaces)
            {
                var sb = new StringBuilder();
                sb.AppendLine("// <auto-generated/>");
                sb.AppendLine("#nullable enable");
                sps.CancellationToken.ThrowIfCancellationRequested();

                SemanticModel? sm;
                try
                {
                    sm = source.Compilation.GetSemanticModel(@interface.SyntaxTree);
                }
                catch (ArgumentException e)
                {
                    if (_logger.IsEnabled)
                        _logger.Write("Generating " + @interface.Identifier.Text + " failed: " + e);
                    continue;
                }

                var interfaceSymbol = sm.GetDeclaredSymbol(@interface);
                if (interfaceSymbol == null)
                {
                    _logger.Write("Generating " + @interface.Identifier.Text + " failed because declared symbol could not be resolved");
                    continue;
                }

                sb.AppendLine("namespace " + interfaceSymbol.ContainingNamespace);
                sb.AppendLine("{");

                sb.AppendLine("    public static class " + @interface.Identifier.Text + "Ex");
                sb.AppendLine("    {");

                var records = new List<string>();
                foreach (var method in @interface.Members.OfType<MethodDeclarationSyntax>())
                {
                    var returnTypeSymbol = sm.GetSymbolInfo(method.ReturnType).Symbol;
                    if (returnTypeSymbol == null)
                    {
                        _logger.Write("Skipped input " + method.Identifier.Text + " because the return typed could not be determined");
                        continue;
                    }

                    var returnTypeFullName = returnTypeSymbol.ToDisplayString();

                    var setParameters = new List<string> { "this " + _ns + ".Db db" };
                    var getParameters = new List<string> { "this " + _ns + ".IDb db" };
                    var removeParameters = new List<string> { "this " + _ns + ".Db db" };
                    var names = new List<string>();
                    var queryTypeProperties = new List<string>();

                    foreach (var parameter in method.ParameterList.Parameters)
                    {
                        if (parameter.Type == null)
                            continue;

                        var symbol = sm.GetSymbolInfo(parameter.Type).Symbol;
                        if (symbol == null)
                            continue;

                        setParameters.Add(symbol.ToDisplayString() + " " + parameter.Identifier);
                        getParameters.Add(symbol.ToDisplayString() + " " + parameter.Identifier);
                        removeParameters.Add(symbol.ToDisplayString() + " " + parameter.Identifier);
                        names.Add(parameter.Identifier.Text);
                        queryTypeProperties.Add(symbol.ToDisplayString() + " " + parameter.Identifier);
                    }

                    setParameters.Add(returnTypeFullName + " result");

                    records.Add("    public record " + method.Identifier.Text + "InputQuery(" + string.Join(", ", queryTypeProperties) + ");");

                    sb.AppendLine();
                    sb.AppendLine("        /// <summary>");
                    sb.AppendLine("        /// Sets the input parameter: " + method.Identifier.Text);
                    sb.AppendLine("        /// </summary>");
                    sb.AppendLine("        public static void Set" + method.Identifier.Text + "(" + string.Join(", ", setParameters) + ")");
                    sb.AppendLine("        {");
                    sb.AppendLine("            db.SetInput(new " + method.Identifier.Text + "InputQuery(" + string.Join(", ", names) + "), result);");
                    sb.AppendLine("        }");
                    sb.AppendLine();

                    sb.AppendLine("        /// <summary>");
                    sb.AppendLine("        /// Returns the input parameter: " + method.Identifier.Text);
                    sb.AppendLine("        /// </summary>");
                    sb.AppendLine("        public static " + returnTypeFullName + " Get" + method.Identifier.Text + "(" + string.Join(", ", getParameters) + ")");
                    sb.AppendLine("        {");
                    sb.AppendLine("            return (" + returnTypeFullName + ")db.Query(new " + method.Identifier.Text + "InputQuery(" + string.Join(", ", names) + "));");
                    sb.AppendLine("        }");
                    sb.AppendLine();

                    sb.AppendLine("        /// <summary>");
                    sb.AppendLine("        /// Removes the input parameter: " + method.Identifier.Text);
                    sb.AppendLine("        /// </summary>");
                    sb.AppendLine("        public static void Remove" + method.Identifier.Text + "(" + string.Join(", ", removeParameters) + ")");
                    sb.AppendLine("        {");
                    sb.AppendLine("            db.RemoveInput(new " + method.Identifier.Text + "InputQuery(" + string.Join(", ", names) + "));");
                    sb.AppendLine("        }");
                    sb.AppendLine();
                }

                sb.AppendLine("    }");

                sb.AppendLine();
                foreach (var record in records)
                {
                    sb.AppendLine(record);
                }
                sb.AppendLine("}");

                sps.AddSource(@interface.Identifier.Text + ".g.cs", sb.ToString());
            }
            _logger.Write("Successfully generated input extensions");
        }
        catch (Exception e)
        {
            _logger.Write("Failed to generate input extensions: " + e);
        }

    }

    private static void BuildMethodSource(SourceProductionContext sps, (Compilation Compilation, ImmutableArray<MethodDeclarationSyntax> Methods) source)
    {
        try
        {
            foreach (var method in source.Methods)
            {
                var sb = new StringBuilder();
                sb.AppendLine("// <auto-generated/>");
                sb.AppendLine("#nullable enable");

                sps.CancellationToken.ThrowIfCancellationRequested();

                SemanticModel? sm;
                try
                {
                    sm = source.Compilation.GetSemanticModel(method.SyntaxTree);
                }
                catch (ArgumentException e)
                {
                    if (_logger.IsEnabled)
                        _logger.Write("Generating " + method.Identifier.Text + " failed: " + e);
                    continue;
                }

                var methodInfo = sm.GetDeclaredSymbol(method);
                if (methodInfo == null)
                {
                    if (_logger.IsEnabled)
                        _logger.Write("Skipped " + method.Identifier.Text + " because the declared symbol could not be resolved.");
                    continue;
                }

                var cachingDisabled = false;
                foreach (var methodAttributeList in method.AttributeLists)
                {
                    foreach (var attributeSyntax in methodAttributeList.Attributes.Where(x => x.Name.ToString() == "DbQuery"))
                    {
                        if (attributeSyntax.ArgumentList?.Arguments.Count == 1)
                            if (attributeSyntax.ArgumentList.Arguments[0].Expression.ToFullString() == "false")
                                cachingDisabled = true;
                    }
                }

                var methodReturnTypeInfo = sm.GetSymbolInfo(method.ReturnType).Symbol as INamedTypeSymbol;

                if (methodReturnTypeInfo == null)
                {
                    if (_logger.IsEnabled)
                        _logger.Write("Skipped " + method.Identifier.Text + " because the return type symbol could not be resolved.");
                    continue;
                }

                var @class = method.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
                if (@class == null)
                {
                    if (_logger.IsEnabled)
                        _logger.Write("Skipped " + method.Identifier.Text + " because the class could not be resolved.");
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
                        if (_logger.IsEnabled)
                            _logger.Write("Skipped " + method.Identifier.Text + " because the parameter " + methodParameter.Identifier.Text + " type was not provided.");
                        failed = true;
                        break;
                    }


                    if (sm.GetSymbolInfo(methodParameter.Type).Symbol is not INamedTypeSymbol typeSymbol)
                    {
                        if (_logger.IsEnabled)
                            _logger.Write("Skipped " + method.Identifier.Text + " because the parameter " + methodParameter.Identifier.Text + " type symbol could not be resolved.");
                        failed = true;
                        break;
                    }


                    if (typeSymbol.ToDisplayString() == _ns + ".IDb")
                        continue;

                    var fullTypeName = typeSymbol.ToDisplayString();
                    if (methodParameter.Type.ToFullString().TrimEnd().EndsWith("?"))
                        fullTypeName += "?";


                    myArguments.Add(methodParameter.Identifier.Text);
                    myMethodParameter.Add(fullTypeName + " " + methodParameter.Identifier.Text);
                    myRecordParameter.Add(fullTypeName + " " + methodParameter.Identifier.Text);
                    handlerArguments.Add("q." + methodParameter.Identifier.Text);
                }

                if (failed)
                    continue;

                handlerArguments.Insert(0, "db");
                myMethodParameter.Insert(0, $"this {_ns}.IDb db");

                var fullReturnType = methodReturnTypeInfo.ToDisplayString() + (method.ReturnType.ToFullString().TrimEnd().EndsWith("?") ? "?" : "");

                if (cachingDisabled)
                {
                    sb.AppendLine($$"""
                    namespace {{methodInfo.ContainingNamespace}}
                    {
                        public record {{method.Identifier.Text}}Input({{string.Join(", ", myRecordParameter)}});
                        
                        public static class {{method.Identifier.Text}}Ex
                        {
                            ///<summary>
                            ///Implementation: <see cref="{{@class.Identifier.Text}}.{{method.Identifier.Text}}" />
                            ///</summary>
                            public static {{fullReturnType}} {{method.Identifier.Text}}({{string.Join(", ", myMethodParameter)}})
                            {
                                return {{@class.Identifier.Text}}.{{method.Identifier.Text}}(db, {{string.Join(", ", myArguments)}});
                            }
                        }
                    }

                    """);
                }
                else
                {
                    sb.AppendLine($$"""
                    namespace {{methodInfo.ContainingNamespace}}
                    {
                        public record {{method.Identifier.Text}}Input({{string.Join(", ", myRecordParameter)}});
                        
                        public static class {{method.Identifier.Text}}Ex
                        {
                            ///<summary>
                            ///Implementation: <see cref="{{@class.Identifier.Text}}.{{method.Identifier.Text}}" />
                            ///</summary>
                            public static {{fullReturnType}} {{method.Identifier.Text}}({{string.Join(", ", myMethodParameter)}})
                            {
                                return ({{fullReturnType}})db.Query(new {{method.Identifier.Text}}Input({{string.Join(",", myArguments)}}));
                            }
                        }

                        public class {{method.Identifier.Text}}GeneratedHandler : {{_ns}}.QueryHandler
                        {
                            public override System.Type HandledType => typeof({{method.Identifier.Text}}Input);

                            public override object Handle({{_ns}}.IDb db, object query)
                            {
                                var q =  ({{method.Identifier.Text}}Input)query;
                                return {{@class.Identifier.Text}}.{{method.Identifier.Text}}({{string.Join(", ", handlerArguments)}});
                            }
                        }
                    }

                    """);
                }
                
                sps.CancellationToken.ThrowIfCancellationRequested();


                try
                {
                    sps.AddSource(method.Identifier.Text + ".DbExtensionMethods.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
                }
                catch (Exception)
                {
                    //Looks like there is a bug(?) where we get the same nodes multiple times.
                    //AddSource will throw if we add the same file multiple times. We can ignore this because
                    //if a node was provided more than once, the code for it was already generated and added.
                }
            }

            _logger.Write("Done");
        }
        catch (Exception e)
        {
            _logger.Write("Done with exception: " + e);
        }
    }

}