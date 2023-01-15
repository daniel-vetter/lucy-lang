using System;
using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record TypeInfo(INodeId<SyntaxTreeNode>? TypeDeclarationNodeId, string Name);
    
    public static class KnownTypes
    {
        public static TypeInfo Void { get; } = new(null, "void");
        public static TypeInfo Int32 { get; } = new(null, "int");
        public static TypeInfo String { get; } = new(null, "string");
    }

    public static class TypeResolving
    {
        public static TypeInfo? GetTypeInfo(this IDb db, INodeId<SyntaxTreeNode> nodeId)
        {
            return nodeId switch
            {
                INodeId<TypeReferenceSyntaxNode> tr => db.GetTypeInfoFromTypeReferenceId(tr),
                INodeId<VariableReferenceExpressionSyntaxNode> vr => db.GetTypeInfoFromVariableReference(vr),
                INodeId<VariableDeclarationStatementSyntaxNode> vd => db.GetTypeInfoFromVariableDeclaration(vd),
                INodeId<ExpressionSyntaxNode> e => db.GetTypeInfoFromExpression(e),
                _ => throw new Exception("Can not resolve type from: " + nodeId.GetType().Name)
            };
        }

        [DbQuery] ///<see cref="GetTypeInfoFromTypeReferenceIdEx.GetTypeInfoFromTypeReferenceId" />
        public static TypeInfo? GetTypeInfoFromTypeReferenceId(IDb db, INodeId<TypeReferenceSyntaxNode> nodeId)
        {
            var node = db.GetNodeById(nodeId);

            return node.TypeName.Text switch
            {
                "int" => KnownTypes.Int32,
                "string" => KnownTypes.String,
                "void" => KnownTypes.Void,
                _ => null
            };
        }

        [DbQuery] ///<see cref="GetTypeInfoFromVariableReferenceEx.GetTypeInfoFromVariableReference" />
        public static TypeInfo? GetTypeInfoFromVariableReference(IDb db, INodeId<VariableReferenceExpressionSyntaxNode> nodeId)
        {
            var declaration = db.GetBestVariableReferenceTarget(nodeId);
            if (declaration == null)
                return null;

            if (declaration is INodeId<VariableDeclarationStatementSyntaxNode> vd)
            {
                return db.GetTypeInfoFromVariableDeclaration(vd);
            }

            throw new Exception("Could not resolve type of: " + declaration.GetType().Name);
        }

        [DbQuery] ///<see cref="GetTypeInfoFromVariableReferenceEx.GetTypeInfoFromVariableReference" />
        public static TypeInfo? GetTypeInfoFromVariableDeclaration(IDb db, INodeId<VariableDeclarationStatementSyntaxNode> nodeId)
        {
            var flat = db.GetFlatVariableDeclaration(nodeId);

            return flat.VariableDefinition.Type != null 
                ? db.GetTypeInfoFromTypeReferenceId(flat.VariableDefinition.Type) 
                : db.GetTypeInfoFromExpression(flat.ExpressionNodeId);
        }

        [DbQuery] ///<see cref="GetExpressionTypeEx.GetExpressionType" />
        public static TypeInfo? GetTypeInfoFromExpression(IDb db, INodeId<ExpressionSyntaxNode> nodeId)
        {
            var node = db.GetNodeById(nodeId);

            switch (node)
            {
                case StringConstantExpressionSyntaxNode:
                    return KnownTypes.String;
                case NumberConstantExpressionSyntaxNode:
                    return KnownTypes.Int32;
                case FunctionCallExpressionSyntaxNode fc:
                    var bestTarget = db.GetBestFunctionCallTarget(fc.NodeId);

                    if (bestTarget == null)
                        return null;

                    if (bestTarget is INodeId<FunctionDeclarationStatementSyntaxNode> fd)
                    {
                        var flatFunctionDeclaration = db.GetFlatFunctionDeclaration(fd);
                        return db.GetTypeInfoFromTypeReferenceId(flatFunctionDeclaration.ReturnType);
                    }

                    throw new ArgumentOutOfRangeException(nameof(node), "Can not resolve type of expression node " + node.GetType().Name + " because it is not a known call target type.");
                case VariableReferenceExpressionSyntaxNode vr:
                    return db.GetTypeInfoFromVariableReference(vr.NodeId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(node), "Can not resolve type of expression node " + node.GetType().Name + " because it is not a known type.");
            }
        }
    }
}
