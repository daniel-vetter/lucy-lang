using System;
using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record TypeInfo(INodeId<SyntaxTreeNode>? TypeDeclarationNodeId, string Name);

    public static class KnownTypes
    {
        public static TypeInfo Void { get; } = new(null, "void");
        public static TypeInfo Int32 { get; } = new(null, "int");
        public static TypeInfo String { get; } = new(null, "string");
    }

    [QueryGroup]
    public class TypeResolver
    {
        private readonly Nodes _nodes;
        private readonly Flats _flats;
        [Inject] private readonly Functions _functions = null!;
        private readonly Variables _variables;

        public TypeResolver(Nodes nodes, Flats flats, Variables variables)
        {
            _nodes = nodes;
            _flats = flats;
            _variables = variables;
        }

        public virtual TypeInfo? GetTypeInfo(INodeId<SyntaxTreeNode> nodeId)
        {
            return nodeId switch
            {
                INodeId<TypeReferenceSyntaxNode> tr => GetTypeInfoFromTypeReference(tr),
                INodeId<VariableReferenceExpressionSyntaxNode> vr => GetTypeInfoFromVariableReference(vr),
                INodeId<VariableDeclarationStatementSyntaxNode> vd => GetTypeInfoFromVariableDeclaration(vd),
                INodeId<ExpressionSyntaxNode> e => GetTypeInfoFromExpression(e),
                _ => throw new Exception("Can not resolve type from: " + nodeId.GetType().Name)
            };
        }

        private TypeInfo? GetTypeInfoFromTypeReference(INodeId<TypeReferenceSyntaxNode> nodeId)
        {
            var node = _nodes.GetNodeById(nodeId);

            return node.TypeName.Text switch
            {
                "int" => KnownTypes.Int32,
                "string" => KnownTypes.String,
                "void" => KnownTypes.Void,
                _ => null
            };
        }

        private TypeInfo? GetTypeInfoFromVariableReference(INodeId<VariableReferenceExpressionSyntaxNode> nodeId)
        {
            var declaration = _variables.GetBestVariableReferenceTarget(nodeId);
            if (declaration == null)
                return null;

            if (declaration is INodeId<VariableDeclarationStatementSyntaxNode> vd)
            {
                return GetTypeInfoFromVariableDeclaration(vd);
            }

            throw new Exception("Could not resolve type of: " + declaration.GetType().Name);
        }

        private TypeInfo? GetTypeInfoFromVariableDeclaration(INodeId<VariableDeclarationStatementSyntaxNode> nodeId)
        {
            var flat = _flats.GetFlatVariableDeclaration(nodeId);

            return flat.VariableDefinition.Type != null
                ? GetTypeInfoFromTypeReference(flat.VariableDefinition.Type)
                : GetTypeInfoFromExpression(flat.ExpressionNodeId);
        }

        private TypeInfo? GetTypeInfoFromExpression(INodeId<ExpressionSyntaxNode> nodeId)
        {
            var node = _nodes.GetNodeById(nodeId);

            switch (node)
            {
                case StringConstantExpressionSyntaxNode:
                    return KnownTypes.String;
                case NumberConstantExpressionSyntaxNode:
                    return KnownTypes.Int32;
                case FunctionCallExpressionSyntaxNode fc:
                    var bestTarget = _functions.GetBestFunctionCallTarget(fc.NodeId);

                    if (bestTarget == null)
                        return null;

                    if (bestTarget is INodeId<FunctionDeclarationStatementSyntaxNode> fd)
                    {
                        var flatFunctionDeclaration = _flats.GetFlatFunctionDeclaration(fd);
                        return GetTypeInfoFromTypeReference(flatFunctionDeclaration.ReturnType);
                    }

                    throw new ArgumentOutOfRangeException(nameof(node),
                        "Can not resolve type of expression node " + node.GetType().Name + " because it is not a known call target type.");
                case VariableReferenceExpressionSyntaxNode vr:
                    return GetTypeInfoFromVariableReference(vr.NodeId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(node),
                        "Can not resolve type of expression node " + node.GetType().Name + " because it is not a known type.");
            }
        }
    }
}