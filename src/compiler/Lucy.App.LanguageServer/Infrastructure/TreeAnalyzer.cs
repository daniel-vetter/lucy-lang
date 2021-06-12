using Lucy.Core.Helper;
using Lucy.Core.Parsing;
using System.Collections.Immutable;
using System.Linq;

namespace Lucy.App.LanguageServer.Infrastructure
{
    public static class TreeAnalyzer
    {
        public static ImmutableArray<SyntaxTreeNode> GetStack(SyntaxTreeNode node, int line, int column)
        {
            var listBuilder = ImmutableArray.CreateBuilder<SyntaxTreeNode>();
            SyntaxTreeNode? Walk(SyntaxTreeNode node)
            {
                listBuilder.Add(node);
                foreach (var child in node.GetChildNodes())
                    if (child.GetRange().Contains(line, column))
                        return Walk(child);
                return null;
            }
            Walk(node);
            listBuilder.Reverse();
            return listBuilder.ToImmutable();
        }

        public static T? FindDeepest<T>(SyntaxTreeNode node, int line, int column, out ImmutableArray<SyntaxTreeNode> stack) where T : SyntaxTreeNode
        {
            var result = GetStack(node, line, column);
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] is not T)
                    continue;

                stack = result.Skip(i).ToImmutableArray();
                return ((T?)result[i]);
            }

            stack = ImmutableArray<SyntaxTreeNode>.Empty;
            return null;
        }

        public static T? FindDeepest<T>(SyntaxTreeNode node, int line, int column) where T : SyntaxTreeNode
        {
            var result = GetStack(node, line, column);
            for (int i=0;i<result.Length;i++)
            {
                if (result[i] is not T)
                    continue;

                return ((T?)result[i]);
            }

            return null;
        }
    }
}
