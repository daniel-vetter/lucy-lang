using Lucy.Common;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.ProjectManagement;
using System.Collections.Immutable;

namespace Lucy.Core.Parsing;

//TODO: General refactoring of this file

public static class Parser
{
    public static DocumentRootSyntaxNode Parse(string documentPath, string content)
    {
        Profiler.Start("Parsing " + documentPath);
        var reader = new Reader(documentPath, content);
        var rootNode = DocumentRootSyntaxNodeParser.ReadDocumentSyntaxNode(reader);
        Profiler.End("Parsing " + documentPath);

        return rootNode;
    }
}

public class ParserResult
{
    private readonly Reader _reader;
    
    public string Code => _reader.Code;
    public DocumentRootSyntaxNode RootNode { get; }

    public static ParserResult CreateFrom(string documentPath, string content)
    {
        return new ParserResult(documentPath, content);
    }

    private ParserResult(string documentPath, string content)
    {
        _reader = new Reader(documentPath, content);
        RootNode = DocumentRootSyntaxNodeParser.ReadDocumentSyntaxNode(_reader);
        IdAssigner.AssignNewIds(documentPath, RootNode);
    }

    private ParserResult(Reader reader, DocumentRootSyntaxNode rootNode)
    {
        _reader = reader;
        RootNode = rootNode;
    }
    
    public ParserResult Update(Range1D range, string newContent)
    {
        var newReader = _reader.Update(range, newContent, out var removedCachedEntries);
        var newRootNode = DocumentRootSyntaxNodeParser.ReadDocumentSyntaxNode(newReader);

        IdAssigner.ReassignIdsFromPreviousTreeOrCreateNewOnes(
            oldTree: RootNode, 
            newTree: newRootNode,
            candidates: removedCachedEntries.OfType<SyntaxTreeNode>().ToImmutableArray()
        );

        return new ParserResult(newReader, newRootNode);
    }
}
