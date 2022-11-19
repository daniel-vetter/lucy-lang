using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Inputs;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record ScopeItem(SyntaxTreeNode Node, ComparableReadOnlyList<ScopeItem> Items);
    
    public static class GetScopeMapHandler
    {
        [GenerateDbExtension] ///<see cref="GetScopeTreeEx.GetScopeTree"/>
        public static ScopeItem GetScopeTree(IDb db, string documentPath)
        {
            var rootNode = db.GetSyntaxTree(documentPath);

            var items = new ComparableReadOnlyList<ScopeItem>.Builder();
            Find(rootNode, items);
            return new ScopeItem(rootNode, items.Build());
        }

        private static void Find(SyntaxTreeNode node, ComparableReadOnlyList<ScopeItem>.Builder items)
        {
            if (node is FunctionDeclarationStatementSyntaxNode)
            {
                var subItems = new ComparableReadOnlyList<ScopeItem>.Builder();
                foreach (var childNode in node.GetChildNodes())
                    Find(childNode, subItems);
                items.Add(new ScopeItem(node, subItems.Build()));
            }
            if (node is FunctionDeclarationParameterSyntaxNode)
            {
                items.Add(new ScopeItem(node, new ComparableReadOnlyList<ScopeItem>()));
            }
            else
            {
                foreach (var childNode in node.GetChildNodes())
                    Find(childNode, items);
            }
        }
    }
}
