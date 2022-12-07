using Lucy.Common;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;

namespace Lucy.Core.Parsing;

public static class Parser
{
    public static DocumentRootSyntaxNodeBuilder Parse(string documentPath, string code)
    {
        Profiler.Start("Parsing " + documentPath);
        var reader = new Code(code);
        var rootNode = DocumentRootSyntaxNodeParser.ReadDocumentSyntaxNode(reader);
        Profiler.End("Parsing " + documentPath);

        Profiler.Start("IdAssign " + documentPath);
        IdAssigner.Run(documentPath, rootNode);
        Profiler.End("IdAssign " + documentPath);
        return rootNode;
    }
}