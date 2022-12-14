using Lucy.Common;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.ProjectManagement;

namespace Lucy.Core.Parsing;

public static class Parser
{
    public static DocumentRootSyntaxNodeBuilder Parse(string documentPath, string code)
    {
        Profiler.Start("Parsing " + documentPath);
        var reader = new Reader(code);
        var rootNode = DocumentRootSyntaxNodeParser.ReadDocumentSyntaxNode(reader);
        Profiler.End("Parsing " + documentPath);

        Profiler.Start("IdAssign " + documentPath);
        IdAssigner.Run(documentPath, rootNode);
        Profiler.End("IdAssign " + documentPath);
        return rootNode;
    }
}

public class ParserResult
{
    private readonly Reader _reader;

    public static ParserResult CreateFrom(string content)
    {
        return new ParserResult(content);
    }

    private ParserResult(string content)
    {
        _reader = new Reader(content);
        RootNode = DocumentRootSyntaxNodeParser.ReadDocumentSyntaxNode(_reader);
    }

    private ParserResult(Reader reader, DocumentRootSyntaxNodeBuilder rootNode)
    {
        _reader = reader;
        RootNode = rootNode;
    }
    
    public ParserResult Update(Range1D range, string newContent)
    {
        var reader = _reader.Update(range, newContent);
        var rootNode = DocumentRootSyntaxNodeParser.ReadDocumentSyntaxNode(_reader);
        return new ParserResult(reader, rootNode);
    }

    public string Code => _reader.Code;
    public DocumentRootSyntaxNodeBuilder RootNode { get; }
}