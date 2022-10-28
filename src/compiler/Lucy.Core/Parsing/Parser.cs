using Lucy.Core.Parsing.Nodes;
using System;

namespace Lucy.Core.Parsing
{
    public static class Parser
    {
        public static DocumentRootSyntaxNode Parse(string documentPath, string code)
        {
            var reader = new Code(code);
            var rootNode = DocumentRootSyntaxNode.ReadDocumentSyntaxNode(reader);
            RangeAssigner.Run(rootNode);
            IdAssigner.Run(documentPath, rootNode);
            return rootNode;
        }
    }
}
