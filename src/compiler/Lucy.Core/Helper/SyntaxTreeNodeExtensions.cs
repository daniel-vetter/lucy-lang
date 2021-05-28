using Lucy.Core.Model.Syntax;
using System.Collections.Generic;

namespace Lucy.Core.Helper
{
    public static class SyntaxTreeNodeExtensions
    {
        public static IEnumerable<SyntaxNode> GetChildNodes(this SyntaxNode node)
        {
            var props = node.GetType().GetProperties();
            foreach(var prop in props)
            {
                var value = prop.GetValue(node);
                if (value == null)
                    continue;

                
                if (value is IEnumerable<SyntaxNode> subList)
                {
                    foreach (var element in subList)
                        yield return element;
                }
                    

                if (value is SyntaxNode parserTreeNode)
                    yield return parserTreeNode;
            }
        }
    }
}
