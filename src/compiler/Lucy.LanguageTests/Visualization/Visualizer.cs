using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Expressions.Unary;

namespace Lucy.LanguageTests.Visualization
{
    internal static class Visualizer
    {
        public static async Task WriteSyntaxTreeToSvg(SyntaxNode node, string path)
        {
            var dot = CreateGraphvisDocument(node);
            var svg = await Graphviz.CreateSvgFromDotFile(dot);
            await File.WriteAllTextAsync(path + ".dot", dot);
            await File.WriteAllBytesAsync(path, svg);
        }

        public static string CreateGraphvisDocument(SyntaxNode node)
        {
            var references = new List<Reference>();
            var sb = new StringBuilder();
            sb.AppendLine("digraph G {");

            void Run(SyntaxNode node)
            {
                foreach (var prop in node.GetType().GetProperties())
                {
                    var value = prop.GetValue(node);
                    if (value == null)
                        continue;

                    if (value is SyntaxNode)
                    {
                        references.Add(new Reference(node, value, prop.Name));
                        if (value is SyntaxNode stn)
                            Run(stn);
                    }
                    if (value is IEnumerable<SyntaxNode> stnl)
                    {
                        int index = 0;
                        foreach (var stni in stnl)
                        {
                            references.Add(new Reference(node, stni, prop.Name + "[" + index + "]"));
                            Run(stni);
                            index++;
                        }
                    }
                }
            }

            Run(node);

            var objIds = new ObjectIds();
            sb.AppendLine("    node [fontname=\"Courier New\" shape=box fontsize=10 ]");
            sb.AppendLine("    edge [fontname=\"Courier New\" fontsize=10]");
            foreach (var r in references)
            {
                sb.AppendLine("    " + objIds.Get(r.From) + " -> " + objIds.Get(r.To) + "[ label=\"" + r.Label + "\" ];");
            }
            sb.AppendLine();
            foreach (var entry in objIds.Entries)
            {
                var graphNode = new TableNode();
                graphNode.Id = objIds.Get(entry);

                if (entry is SyntaxNode)
                {
                    graphNode.Color = "008800";
                    graphNode.Title = entry.GetType().Name;

                    if (entry is StringConstantExpressionSyntaxNode sc) graphNode.Properties.Add(new Property("Value:", sc.Value));
                }

                if (entry is TokenNode token)
                {
                    graphNode.Color = "0000FF";
                    graphNode.Title = "Token";
                    graphNode.Properties.Add(new Property("Value:", token.Value));
                    graphNode.Properties.Add(new Property("Range:", token.Range.Position + "-" + (token.Range.Position + token.Range.Length)));
                    graphNode.Properties.Add(new Property("Trivia:", token.LeadingTrivia.Aggregate(0, (x, y) => x + y.Length).ToString()));
                    graphNode.Title = "\"" + token.Value + "\"";
                }

                sb.AppendLine("    " + graphNode);
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }

    internal record Reference(object From, object To, string Label)
    {

    }
    internal class ObjectIds
    {
        List<object> _list = new List<object>();

        public string Get(object obj)
        {
            int index = -1;
            for (int i = 0; i < _list.Count; i++)
            {
                if (ReferenceEquals(_list[i], obj))
                {
                    index = i;
                    break;
                }

            }

            if (index == -1)
            {
                index = _list.Count;
                _list.Add(obj);
            }

            return "o" + index + "_" + obj?.GetType().Name;
        }

        public ImmutableArray<object> Entries => _list.ToImmutableArray();
    }
}
