﻿using Lucy.Core.Model;
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

    public static class Types
    {
        [GenerateDbExtension] ///<see cref="GetTypeInfoFromTypeReferenceIdEx.GetTypeInfoFromTypeReferenceId" />
        public static TypeInfo? GetTypeInfoFromTypeReferenceId(IDb db, INodeId<TypeReferenceSyntaxNode> nodeId)
        {
            var node = db.GetNodeById(nodeId);

            return node.TypeName.Token.Text switch
            {
                "int" => KnownTypes.Int32,
                "string" => KnownTypes.String,
                "void" => KnownTypes.Void,
                _ => null
            };
        }
    }
}
