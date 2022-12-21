using System.Linq;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record FlatVariableDeclaration(INodeId<VariableDeclarationStatementSyntaxNode> NodeId, FlatIdentifier Name, INodeId<ExpressionSyntaxNode> ExpressionNodeId);

    internal static class VariablesHandler
    {
        [GenerateDbExtension] ///<see cref="GetFlatVariableDeclarationEx.GetFlatVariableDeclaration" />
        public static FlatVariableDeclaration GetFlatVariableDeclaration(IDb db, INodeId<VariableDeclarationStatementSyntaxNode> nodeId)
        {
            var node = db.GetNodeById(nodeId);

            return new FlatVariableDeclaration(
                node.NodeId,
                new FlatIdentifier(node.VariableName.Text, node.VariableName.NodeId),
                node.Expression.NodeId
            );
        }

        [GenerateDbExtension] ///<see cref="GetAvailableFunctionsInScopeEx.GetAvailableFunctionsInScope" />
        public static ComparableReadOnlyList<FlatVariableDeclaration> GetAvailableVariablesInScope(IDb db, INodeId<SyntaxTreeNode> scopeTarget)
        {
            var currentNode = scopeTarget;
            var result = new ComparableReadOnlyList<FlatVariableDeclaration>.Builder();
            
            while (true)
            {
                var parent = db.GetParentNodeIdOfTypes<StatementListSyntaxNode, FunctionDeclarationStatementSyntaxNode>(currentNode);
                if (parent == null)
                    break;

                if (parent is INodeId<StatementListSyntaxNode> statementListSyntaxNodeId)
                {
                    var list = db
                        .GetNodeIdsByTypeInStatementList<VariableDeclarationStatementSyntaxNode>(statementListSyntaxNodeId)
                        .Select(db.GetFlatVariableDeclaration);

                    result.AddRange(list);
                }

                if (parent is INodeId<FunctionDeclarationStatementSyntaxNode>)
                {
                    //TODO: handle function parameters
                    break;
                }
                
                currentNode = parent;
            }

            return result.Build();
        }
    }
}
