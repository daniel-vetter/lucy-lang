using System;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record FlatVariableDefinition(
        INodeId<VariableDefinitionSyntaxNode> NodeId,
        FlatIdentifier Name,
        INodeId<ExpressionSyntaxNode> ExpressionNodeId);

    internal static class VariablesHandler
    {
        [DbQuery] ///<see cref="GetFlatVariableDeclarationEx.GetFlatVariableDeclaration" />
        public static FlatVariableDefinition GetFlatVariableDeclaration(IDb db, INodeId<VariableDeclarationStatementSyntaxNode> nodeId)
        {
            var node = db.GetNodeById(nodeId);

            return new FlatVariableDefinition(
                node.VariableDefinition.NodeId,
                new FlatIdentifier(node.VariableDefinition.VariableName.Text, node.VariableDefinition.VariableName.NodeId),
                node.Expression.NodeId
            );
        }

        [DbQuery] ///<see cref="GetReachableVariablesInStatementListEx.GetReachableVariablesInStatementList" />
        public static ComparableReadOnlyList<FlatVariableDefinition> GetReachableVariablesInStatementList(IDb db, INodeId<StatementListSyntaxNode> statementListNodeId)
        {
            var currentNode = statementListNodeId;
            var result = new ComparableReadOnlyList<FlatVariableDefinition>.Builder();
            while (true)
            {
                result.AddRange(db.GetDeclaredVariablesInStatementList(currentNode));

                currentNode = db.GetParentNodeIdOfType<StatementListSyntaxNode>(currentNode);
                if (currentNode == null)
                    break;
            }
            return result.Build();
        }

        [DbQuery] ///<see cref="GetDeclaredVariablesInStatementListEx.GetDeclaredVariablesInStatementList" />
        public static ComparableReadOnlyList<FlatVariableDefinition> GetDeclaredVariablesInStatementList(IDb db, INodeId<StatementListSyntaxNode> statementListNodeId)
        {
            var result = new ComparableReadOnlyList<FlatVariableDefinition>.Builder();
            void Traverse(SyntaxTreeNode node)
            {
                if (node is VariableDeclarationStatementSyntaxNode vd)
                    result.Add(db.GetFlatVariableDeclaration(vd.NodeId));

                if (node is FunctionDeclarationStatementSyntaxNode fd)
                {

                }
                
                foreach (var child in node.GetChildNodes())
                {
                    if (child is not FunctionDeclarationStatementSyntaxNode and not StatementListSyntaxNode)
                        Traverse(child);
                }
            }

            foreach (var statement in db.GetNodeById(statementListNodeId).Statements)
                Traverse(statement);

            return result.Build();
        }


        [DbQuery] ///<see cref="GetReachableFunctionsInScopeEx.GetReachableFunctionsInScope" />
        public static ComparableReadOnlyList<FlatVariableDefinition> GetReachableVariablesInScope(IDb db, INodeId<SyntaxTreeNode> scopeTarget)
        {
            var sl = scopeTarget as INodeId<StatementListSyntaxNode>
                     ?? db.GetParentNodeIdOfType<StatementListSyntaxNode>(scopeTarget);

            if (sl is null)
                throw new Exception("Invalid scopeTarget");

            return db.GetReachableVariablesInStatementList(sl);
        }

        public static INodeId<SyntaxTreeNode>? GetScopeId(IDb db, INodeId<SyntaxTreeNode> nodeId)
        {
            INodeId<SyntaxTreeNode>? current = nodeId;
            while (true)
            {
                if (current is INodeId<StatementListSyntaxNode> slNodeId)
                    return slNodeId;

                if (current is INodeId<FunctionDeclarationStatementSyntaxNode> fdNodeId)
                {
                    var fd = db.GetNodeById(fdNodeId);
                    return fd.FunKeyword.NodeId;
                }
                current = db.GetParentNodeId(nodeId);
                if (current == null)
                    return null;
            }
            

            
        }
    }
}
