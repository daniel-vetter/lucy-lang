﻿using Microsoft.CodeAnalysis;
using System.Text;

namespace Lucy.Core.SourceGenerator
{
    internal class ModelGenerator
    {
        internal static void Generate(SourceProductionContext productionContext, string name, Config config)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#nullable enable");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Collections.Immutable;");
            sb.AppendLine();
            sb.AppendLine($"namespace {config.Namespace};");
            sb.AppendLine();
            foreach (var node in config.Nodes)
            {
                sb.AppendLine($"public {(!node.IsTopMost).Then("abstract ")}class {node.Name}{((!node.IsRoot).Then(" : " + node.BasedOn))}");
                sb.AppendLine("{");
                WriteConstructor(sb, node);
                WriteProperties(sb, node);
                WriteGetChildNodesMethod(sb, node);
                WriteToImmutableMethod(sb, node);
                sb.AppendLine("}");
                sb.AppendLine();
            }

            productionContext.AddSource(name + ".g.cs", sb.ToString());
        }

        private static void WriteToImmutableMethod(StringBuilder sb, Node node)
        {
            if (node.IsTopMost)
            {
                var props = node.AllProperties
                        .Select(x =>
                        {
                            var name = x.Name;

                            if ((x.IsList || x.TypeIsNode) && x.IsOptional)
                                name += "?";

                            if (x.IsList && x.TypeIsNode)
                                name += ".Select(x => x.ToImmutable())";

                            if (x.IsList)
                                name += ".ToImmutableArray()";
                            else if (x.TypeIsNode)
                                name += ".ToImmutable()";

                            return name;
                        })
                        .ToArray();

                sb.AppendLine($"    public override Immutable{node.Name} ToImmutable()");
                sb.AppendLine("    {");
                sb.AppendLine($"        return new Immutable{node.Name}({string.Join(", ", props)});");
                sb.AppendLine("    }");
            }
            else
            {
                sb.AppendLine($"    public abstract {(!node.IsRoot).Then("override ")}Immutable{node.Name} ToImmutable();");
            }
        }

        private static void WriteGetChildNodesMethod(StringBuilder sb, Node node)
        {
            if (node.IsRoot)
            {
                sb.AppendLine("    public abstract IEnumerable<SyntaxTreeNode> GetChildNodes();");
            }
            else
            {
                if (!node.IsTopMost)
                    return;

                sb.AppendLine("    public override IEnumerable<SyntaxTreeNode> GetChildNodes()");
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

        private static void WriteProperties(StringBuilder sb, Node node)
        {
            if (node.Properties.Count == 0)
                return;

            foreach (var prop in node.Properties)
                sb.AppendLine($"    public {GetRealType(prop)} {prop.Name} {{ get; set; }}{(prop.Init != null).Then(" = " + prop.Init + ";")}");
            sb.AppendLine();
        }

        private static void WriteConstructor(StringBuilder sb, Node node)
        {
            if (node.Properties.Count == 0)
                return;

            sb.AppendLine("    public " + node.Name + "(" + GetConstructorArguments(node) + ")");
            sb.AppendLine("    {");
            foreach (var prop in node.Properties.Where(x => x.Init == null))
            {
                sb.AppendLine("        " + prop.Name + " = " + ToLower(prop.Name) + ";");
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
}
