using Lucy.Core.Model.Syntax;
using System.Collections.Generic;

namespace Lucy.Core.Helper
{
    public static class SyntaxTreeNodeExtensions
    {
        public static IEnumerable<SyntaxNodeChild> GetChildNodes(this SyntaxNode node)
        {
            var props = node.GetType().GetProperties();
            foreach(var prop in props)
            {
                var value = prop.GetValue(node);
                if (value == null)
                    continue;

                
                if (value is IEnumerable<SyntaxNode> subList)
                {
                    int i = 0;
                    foreach (var element in subList)
                        yield return new SyntaxNodeChild($"{prop.Name}[{i++}]", element);
                }
                    

                if (value is SyntaxNode parserTreeNode)
                    yield return new SyntaxNodeChild(prop.Name, parserTreeNode);
            }
        }
    }

    public record SyntaxNodeChild(string Name, SyntaxNode Node);
}
