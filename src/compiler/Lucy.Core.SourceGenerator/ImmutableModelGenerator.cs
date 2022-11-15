﻿using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace Lucy.Core.SourceGenerator
{
    internal class ImmutableModelGenerator
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
            sb.AppendLine("using Lucy.Core.Parsing.Nodes;");
            sb.AppendLine();
            sb.AppendLine("namespace " + config.Namespace + ";");
            sb.AppendLine();

            foreach (var node in config.Nodes)
            {
                var basedOn = new List<string>();
                if (node.BasedOn != null)
                    basedOn.Add("Immutable" + node.BasedOn);
                if (node.IsRoot)
                    basedOn.Add("IHashable");
                basedOn.Add("IEquatable<Immutable" + node.Name + "?>");

                sb.AppendLine($"public {(node.IsTopMost ? "" : "abstract ")}class Immutable{node.Name} : " + string.Join(", ", basedOn));
                sb.AppendLine("{");
                WriteConstructor(sb, config, node);
                WriteProperties(sb, config, node);
                WriteMemberVariables(sb, node);
                WriteGetChildNodesMethod(config, sb, node);
                //WriteToFlatMethod(config, sb, node);
                WriteHashBuilder(sb, node);
                WriteEqualsMethods(sb, node);
                sb.AppendLine("}");
                sb.AppendLine();
            }

            productionContext.AddSource("Immutable" + name + ".g.cs", sb.ToString());
        }

        private static void WriteEqualsMethods(StringBuilder sb, Node node)
        {
            
            sb.AppendLine("    public bool Equals(Immutable" + node.Name + "? other)");
            sb.AppendLine("    {");
            sb.AppendLine("        return other is not null && _hash.AsSpan().SequenceEqual(other._hash);");
            sb.AppendLine("    }");
            sb.AppendLine();
            
            if (node.IsRoot)
            {
                sb.AppendLine("    public override bool Equals(object? obj)");
                sb.AppendLine("    {");
                sb.AppendLine($"        return Equals(obj as Immutable{node.Name});");
                sb.AppendLine("    }");
                sb.AppendLine();

                sb.AppendLine("    public override int GetHashCode()");
                sb.AppendLine("    {");
                sb.AppendLine("        return _hashShort;");
                sb.AppendLine("    }");
                sb.AppendLine();
            }
        }

        private static void WriteMemberVariables(StringBuilder sb, Node node)
        {
            if (!node.IsRoot)
                return;

            sb.AppendLine("    protected byte[] _hash = null!;");
            sb.AppendLine("    protected int _hashShort;");
            sb.AppendLine();
        }

        private static void WriteHashBuilder(StringBuilder sb, Node node)
        {
            if (node.IsRoot)
            {
                sb.AppendLine("    protected abstract byte[] BuildHash();");
                sb.AppendLine();
                sb.AppendLine("    protected void EnsureHashIsBuild()");
                sb.AppendLine("    {");
                sb.AppendLine("        _hash = BuildHash();");
                sb.AppendLine();
                sb.AppendLine("        var hc = new HashCode();");
                sb.AppendLine("        hc.AddBytes(GetFullHash());");
                sb.AppendLine("        _hashShort = hc.ToHashCode();");
                sb.AppendLine("    }");
                sb.AppendLine();
                sb.AppendLine("    public byte[] GetFullHash()");
                sb.AppendLine("    {");
                sb.AppendLine("        return _hash;");
                sb.AppendLine("    }");
                sb.AppendLine();
            }
                

            if (node.IsTopMost)
            {
                sb.AppendLine("    protected override byte[] BuildHash()");
                sb.AppendLine("    {");
                sb.AppendLine("        using var b = new HashBuilder();");
                sb.AppendLine("        b.Add(" + node.Index + ");");
                foreach (var property in node.AllProperties)
                {
                    if (property.IsList)
                    {
                        sb.AppendLine("        b.BeginList();");
                        sb.AppendLine("        foreach(var entry in " + property.Name + ")");
                        sb.AppendLine("            b.Add(entry);");
                    }
                    else
                        sb.AppendLine("        b.Add(" + property.Name + ");");
                }
                sb.AppendLine("        return b.Build();");
                sb.AppendLine("    }");
                sb.AppendLine();
            }
        }

        private static void WriteConstructor(StringBuilder sb, Config config, Node node)
        {
            var allParameters = node
                .Properties
                .Concat(GetBaseProperties(config, node))
                .Select(x => $"{GetRealType(config, x)} {ToLower(x.Name)}")
                .ToArray();

            var baseParameters = GetBaseProperties(config, node)
                .Select(x => ToLower(x.Name))
                .ToArray();

            var paramStr = string.Join(", ", allParameters);
            var baseStr = string.Join(", ", baseParameters);

            sb.AppendLine($"    public Immutable{node.Name}({paramStr}) : base(" + baseStr + ")");
            sb.AppendLine("    {");
            foreach (var prop in node.Properties)
            {
                sb.AppendLine($"        {prop.Name} = {ToLower(prop.Name)};");
            }
            if (node.IsTopMost)
                sb.AppendLine("        EnsureHashIsBuild();");
            sb.AppendLine("    }");
            sb.AppendLine();
        }


        private static void WriteToFlatMethod(Config config, StringBuilder sb, Node node)
        {
            if (node.BasedOn == null)
            {
                sb.AppendLine($"    public abstract Flat{node.Name} ToFlat();");
            }
            else
            {
                if (!node.IsTopMost)
                {
                    sb.AppendLine($"    public abstract override Flat{node.Name} ToFlat();");
                }
                else
                {
                    var props = GetAllProperties(config, node)
                        .Select(x =>
                        {
                            var name = x.Name;

                            if (x.IsList)
                            {
                                if (IsSyntaxTreeNode(config, x.Type))
                                    name = name + (x.IsOptional ? "?" : "") + ".Select(x => x.NodeId.ToTyped<Flat" + x.Type + ">()).ToImmutableArray()";
                            }
                            else
                            {
                                if (IsSyntaxTreeNode(config, x.Type))
                                    name = name + (x.IsOptional ? "?" : "") + ".NodeId.ToTyped<Flat" + x.Type + ">()";
                            }

                            return name;
                        })
                        .ToArray();

                    sb.AppendLine($"    public override Flat{node.Name} ToFlat()");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        return new Flat{node.Name}({string.Join(", ", props)});");
                    sb.AppendLine("    }");
                }
            }
            sb.AppendLine();
        }


        private static ImmutableArray<NodeProperty> GetAllProperties(Config config, Node node)
        {
            var b = ImmutableArray.CreateBuilder<NodeProperty>();
            while (true)
            {
                b.AddRange(node.Properties);

                if (node.BasedOn == null)
                    break;

                node = config.Nodes.FirstOrDefault(x => x.Name == node.BasedOn) ?? throw new Exception("Could not find node \"" + node.BasedOn + "\" when looking for base node for \"" + node.Name + "\"");
            }
            return b.ToImmutable();
        }

        private static string ToLower(string value)
        {
            return value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
        }

        private static string GetRealType(Config config, NodeProperty property)
        {
            var name = property.Type;

            if (IsSyntaxTreeNode(config, property.Type))
                name = "Immutable" + name;

            if (property.IsOptional)
                name += "?";

            if (property.IsList)
                name = "ImmutableArray<" + name + ">";

            return name;
        }

        private static void WriteProperties(StringBuilder sb, Config config, Node node)
        {
            if (node.Properties.Count == 0)
                return;

            foreach (var prop in node.Properties)
            {
                sb.AppendLine($"    public {GetRealType(config, prop)} {prop.Name} {{ get; }}");
            }

            sb.AppendLine();
        }

        private static void WriteGetChildNodesMethod(Config config, StringBuilder sb, Node node)
        {
            if (node.BasedOn == null)
            {
                sb.AppendLine("    public abstract IEnumerable<ImmutableSyntaxTreeNode> GetChildNodes();");
            }
            else
            {
                if (!node.IsTopMost)
                    return;

                sb.AppendLine("    public override IEnumerable<ImmutableSyntaxTreeNode> GetChildNodes()");
                sb.AppendLine("    {");
                int count = 0;
                foreach (var prop in node.Properties)
                {
                    if (!IsSyntaxTreeNode(config, prop.Type))
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

        private static ImmutableArray<NodeProperty> GetBaseProperties(Config config, Node node)
        {
            if (node.BasedOn == null)
                return ImmutableArray<NodeProperty>.Empty;

            var b = ImmutableArray.CreateBuilder<NodeProperty>();
            while (true)
            {
                node = config.Nodes.FirstOrDefault(x => x.Name == node.BasedOn) ?? throw new Exception("Could not find node \"" + node.BasedOn + "\" when looking for base node for \"" + node.Name + "\"");

                b.AddRange(node.Properties);

                if (node.BasedOn == null)
                    break;
            }
            return b.ToImmutable();
        }

        private static bool IsSyntaxTreeNode(Config config, string nodeName)
        {
            var node = config
                .Nodes
                .FirstOrDefault(x => x.Name == nodeName);

            return node != null;
        }
    }
}
