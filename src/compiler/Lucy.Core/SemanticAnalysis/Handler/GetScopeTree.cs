using Lucy.Core.Helper;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using Lucy.Core.SemanticAnalysis.Inputs;
using System.Diagnostics;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetScopeTree(string DocumentPath) : IQuery<GetScopeTreeResult>;
    public record GetScopeTreeResult(DocumentScope DocumentScope);


    
    public record DocumentScope(NodeId NodeId, ComparableReadOnlyList<ScopeItem> Items) : ScopeItem(NodeId);

    public abstract record ScopeItem(NodeId NodeId);
    public record VariableDeclarationScopeItem(NodeId NodeId) : ScopeItem(NodeId);
    public record FunctionDeclarationScopeItem(NodeId NodeId, ComparableReadOnlyList<ScopeItem> Items) : ScopeItem(NodeId);
    
    public class GetScopeMapHandler : QueryHandler<GetScopeTree, GetScopeTreeResult>
    {
        public override GetScopeTreeResult Handle(IDb db, GetScopeTree query)
        {
            var rootNode = db.Query(new GetSyntaxTree(query.DocumentPath)).RootNode;

            var items = new ComparableReadOnlyList<ScopeItem>.Builder();
            Find(rootNode, items);
            var documentScope = new DocumentScope(rootNode.NodeId, items.Build());
            return new GetScopeTreeResult(documentScope);
        }

        void Find(SyntaxTreeNode node, ComparableReadOnlyList<ScopeItem>.Builder items)
        {
            if (node is FunctionDeclarationStatementSyntaxNode)
            {
                var subItems = new ComparableReadOnlyList<ScopeItem>.Builder();
                foreach (var childNode in node.GetChildNodes())
                    Find(childNode, subItems);
                items.Add(new FunctionDeclarationScopeItem(node.NodeId, subItems.Build()));
            }
            if (node is FunctionDeclarationParameterSyntaxNode)
            {
                items.Add(new VariableDeclarationScopeItem(node.NodeId));
            }
            else
            {
                foreach (var childNode in node.GetChildNodes())
                    Find(childNode, items);
            }
        }
    }


}
