using Lucy.Core.Parsing.Nodes;

namespace Lucy.Core.Parsing
{
    public static class Parser
    {
        public static DocumentSyntaxNode Parse(string code)
        {
            var reader = new Code(code);
            var rootNode = DocumentSyntaxNode.ReadDocumentSyntaxNode(reader);
            RangeAssigner.Run(rootNode);
            return rootNode;
        }
    }
}
