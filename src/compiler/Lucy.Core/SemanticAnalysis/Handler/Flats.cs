using System.Linq;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record FlatIdentifier(string Text, INodeId<TokenNode> NodeId);
    public record FlatFunctionDeclaration(INodeId<FunctionDeclarationStatementSyntaxNode> NodeId, FlatIdentifier Name, ComparableReadOnlyList<FlatFunctionDeclarationParameter> Parameters, INodeId<TypeReferenceSyntaxNode> ReturnType);
    public record FlatFunctionDeclarationParameter(INodeId<FunctionDeclarationParameterSyntaxNode> NodeId, FlatIdentifier Name, INodeId<TypeReferenceSyntaxNode>? TypeReference);
    public record FlatFunctionCall(INodeId<FunctionCallExpressionSyntaxNode> NodeId, FlatIdentifier Name, ComparableReadOnlyList<INodeId<ExpressionSyntaxNode>> Arguments);
    public record FlatFunctionParameterDeclaration(INodeId<FunctionDeclarationParameterSyntaxNode> NodeId, INodeId<TypeReferenceSyntaxNode> TypeReferenceNodeId, FlatIdentifier Name);
    public record FlatVariableDeclaration(INodeId<VariableDeclarationStatementSyntaxNode> NodeId, FlatVariableDefinition VariableDefinition, INodeId<ExpressionSyntaxNode> ExpressionNodeId);
    public record FlatVariableDefinition(INodeId<VariableDefinitionSyntaxNode> NodeId, FlatIdentifier Name, INodeId<TypeReferenceSyntaxNode>? Type);

    [QueryGroup]
    public class Flats
    {
        private readonly Nodes _nodes;

        public Flats(Nodes nodes)
        {
            _nodes = nodes;
        }

        public virtual FlatFunctionCall GetFlatFunctionCall(INodeId<FunctionCallExpressionSyntaxNode> functionCallNodeId)
        {
            var node = _nodes.GetNodeById(functionCallNodeId);

            return new FlatFunctionCall(
                functionCallNodeId,
                new FlatIdentifier(node.FunctionName.Text, node.FunctionName.NodeId),
                node.ArgumentList.Select(x => x.Expression.NodeId).ToComparableReadOnlyList()
            );
        }

        public virtual FlatFunctionDeclaration GetFlatFunctionDeclaration(INodeId<FunctionDeclarationStatementSyntaxNode> functionDeclarationNodeId)
        {
            var node = _nodes.GetNodeById(functionDeclarationNodeId);

            var parameters = node.ParameterList
                .Select(x => new FlatFunctionDeclarationParameter(
                    x.NodeId,
                    new FlatIdentifier(
                        x.VariableDefinition.VariableName.Text,
                        x.VariableDefinition.VariableName.NodeId
                    ),
                    x.VariableDefinition.VariableType?.TypeReference.NodeId
                )).ToComparableReadOnlyList();

            return new FlatFunctionDeclaration(
                node.NodeId,
                new FlatIdentifier(
                    node.FunctionName.Text,
                    node.FunctionName.NodeId
                ),
                parameters,
                node.ReturnType.TypeReference.NodeId
            );
        }

        public virtual FlatVariableDefinition GetFlatVariableDefinition(INodeId<VariableDefinitionSyntaxNode> nodeId)
        {
            var node = _nodes.GetNodeById(nodeId);

            return new FlatVariableDefinition(
                node.NodeId,
                new FlatIdentifier(node.VariableName.Text, node.VariableName.NodeId),
                node.VariableType?.TypeReference.NodeId
            );
        }

        public virtual FlatVariableDeclaration GetFlatVariableDeclaration(INodeId<VariableDeclarationStatementSyntaxNode> nodeId)
        {
            var node = _nodes.GetNodeById(nodeId);

            return new FlatVariableDeclaration(
                nodeId,
                GetFlatVariableDefinition(node.VariableDefinition.NodeId),
                node.Expression.NodeId
            );
        }
    }
}