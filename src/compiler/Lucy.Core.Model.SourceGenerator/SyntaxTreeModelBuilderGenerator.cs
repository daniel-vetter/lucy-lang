﻿using Microsoft.CodeAnalysis;
using System.Text;

namespace Lucy.Core.Model.SourceGenerator;

internal static class SyntaxTreeModelBuilderGenerator
{
    internal static void Generate(SourceProductionContext productionContext, Config config)
    {
        var sb = new StringBuilder();
        sb.AppendLine($$"""
                // <auto-generated/>
                #nullable enable
                using System;
                using System.Linq;
                using System.Collections.Generic;
                using System.Collections.Immutable;

                namespace {{config.Namespace}};

                """);

        foreach (var node in config.Nodes)
        {
            sb.WriteClass(
                name: node.Name + "Builder",
                isAbstract: !node.IsTopMost,
                baseClass: node.BasedOn != null ? node.BasedOn + "Builder" : null,
                content: x =>
                {
                    x.WriteConstructor(node);
                    x.WriteProperties(node);
                    x.WriteGetChildNodesMethod(node);
                    x.WriteBuildMethod(node);
                });
        }

        productionContext.AddSource("SyntaxTreeModelBuilder.g.cs", sb.ToString());
    }

    private static void WriteBuildMethod(this StringBuilder sb, Node node)
    {
        if (node.IsTopMost)
        {
            var props = string.Join(", ", node.AllProperties
                .Select(x =>
                {
                    var name = x.Name;

                    if ((x.IsList || x.TypeIsNode) && x.IsOptional)
                        name += "?";

                    if (x.IsList && x.TypeIsNode)
                        name += ".Select(x => x.Build())";

                    if (x.IsList)
                        name += ".ToImmutableArray()";
                    else if (x.TypeIsNode)
                        name += ".Build()";

                    return name;
                })
                .ToArray());

            sb.AppendLine($$"""
                        public override {{node.Name}} Build()
                        {
                            return new {{node.Name}}({{props}});
                        }
                    """);
        }
        else
        {
            sb.AppendLine($"    public abstract {(!node.IsRoot).Then("override ")}{node.Name} Build();");
        }
    }

    private static void WriteGetChildNodesMethod(this StringBuilder sb, Node node)
    {
        if (node.IsRoot)
        {
            sb.AppendLine("    public abstract IEnumerable<SyntaxTreeNodeBuilder> GetChildNodes();");
        }
        else
        {
            if (!node.IsTopMost)
                return;

            sb.AppendLine("    public override IEnumerable<SyntaxTreeNodeBuilder> GetChildNodes()");
            sb.AppendLine("    {");
            int count = 0;
            foreach (var prop in node.Properties)
            {
                if (!prop.TypeIsNode)
                    continue;

                var padding = "        ";
                if (prop.IsList)
                {
                    sb.AppendLine(padding + "foreach (var entry in " + prop.Name + ")");
                    padding += "    ";
                }

                if (prop.IsOptional)
                {
                    sb.AppendLine(padding + "if (" + prop.Name + " != null)");
                    padding += "    ";

                }

                sb.AppendLine(padding + "yield return " + (prop.IsList ? "entry" : prop.Name) + ";");
                count++;
            }

            if (count == 0)
                sb.AppendLine("        yield break;");

            sb.AppendLine("    }");
        }
        sb.AppendLine();
    }

    private static void WriteProperties(this StringBuilder sb, Node node)
    {
        if (node.Properties.Count == 0)
            return;

        foreach (var prop in node.Properties)
            sb.AppendLine($"    public {GetRealType(prop)} {prop.Name} {{ get; set; }}{(prop.Init != null).Then(" = " + prop.Init + ";")}");
        sb.AppendLine();
    }

    private static void WriteConstructor(this StringBuilder sb, Node node)
    {
        if (node.Properties.Count == 0)
            return;

        sb.AppendLine($"    public {node.Name}Builder({GetConstructorArguments(node)})");
        sb.AppendLine("    {");
        foreach (var prop in node.Properties.Where(x => x.Init == null))
        {
            sb.AppendLine($"        {prop.Name} = {ToLower(prop.Name)};");
        }
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static string GetConstructorArguments(Node node)
    {
        return string.Join(", ", node.Properties.Where(x => x.Init == null).Select(x => GetRealType(x) + " " + ToLower(x.Name)));
    }

    private static string GetRealType(NodeProperty property)
    {
        var name = property.Type;

        if (property.TypeIsNode)
            name += "Builder";

        if (property.IsList)
            name = $"List<{name}>";

        if (property.IsOptional)
            name = name + "?";

        return name;
    }

    private static string ToLower(string value)
    {
        return value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
    }
}