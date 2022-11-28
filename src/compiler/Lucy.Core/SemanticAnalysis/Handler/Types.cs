using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record TypeInfo(NodeId? TypeDeclarationNodeId, string Name);
    
    public static class PredefinedTypes
    {
        public static TypeInfo Void { get; } = new(null, "void");
        public static TypeInfo Int32 { get; } = new(null, "int");
        public static TypeInfo String { get; } = new(null, "string");
    }

    public static class Types
    {
        [GenerateDbExtension] ///<see cref="GetTypeInfoFromTypeReferenceIdEx.GetTypeInfoFromTypeReferenceId" />
        public static TypeInfo? GetTypeInfoFromTypeReferenceId(IDb db, NodeId nodeId)
        {
            var node = (TypeReferenceSyntaxNode)db.GetNodeById(nodeId);

            return node.TypeName.Token.Text switch
            {
                "int" => PredefinedTypes.Int32,
                "string" => PredefinedTypes.String,
                "void" => PredefinedTypes.Void,
                _ => null
            };
        }
    }
}
