using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;

namespace Lucy.Core.Parsing;

public static class Parser
{
    public static DocumentRootSyntaxNodeBuilder Parse(string documentPath, string code)
    {
        var reader = new Code(code);
        var rootNode = DocumentRootSyntaxNodeParser.ReadDocumentSyntaxNode(reader);
        IdAssigner.Run(documentPath, rootNode);

        return rootNode;
    }
}