using Lucy.Core.Parsing;
using System.Collections.Generic;

namespace Lucy.Core.Helper
{
    public static class SyntaxTreeNodeExtensions
    {
        //TODO: measure if this is slow and maybe find something faster (source generator?)
        public static IEnumerable<SyntaxTreeNode> GetChildNodes(this SyntaxTreeNode node)
        {
            var props = node.GetType().GetProperties();
            foreach(var prop in props)
            {
                var value = prop.GetValue(node);
                if (value == null)
                    continue;

                
                if (value is IEnumerable<SyntaxTreeNode> subList)
                {
                    foreach (var element in subList)
                        yield return element;
                }
                    

                if (value is SyntaxTreeNode parserTreeNode)
                    yield return parserTreeNode;
            }
        }
    }
}
