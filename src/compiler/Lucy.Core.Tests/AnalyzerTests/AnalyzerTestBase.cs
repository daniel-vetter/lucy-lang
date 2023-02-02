using System.Collections.Immutable;
using System.Diagnostics;
using Lucy.Core.Model;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.SemanticAnalysis.Handler;

namespace Lucy.Core.Tests.AnalyzerTests;

public class AnalyzerTestBase
{
    private Workspace _ws = new();
    private readonly List<(string Path, Position1D Position)> _cursorPositions = new();
    private SemanticAnalyzer? _sa;

    [SetUp]
    public void Setup()
    {
        _sa?.Dispose();
        _sa = null;
        _cursorPositions.Clear();
        _ws = new Workspace();
    }

    protected void AddDoc(string content)
    {
        AddDoc($"/file{_ws.Documents.Count}.lucy", content);
    }

    protected void AddDoc(string path, string content)
    {
        while (true)
        {
            var pos = content.IndexOf("#", StringComparison.InvariantCulture);
            if (pos == -1)
                break;

            _cursorPositions.Add((path, new Position1D(pos)));

            content = content.Remove(pos, 1);
        }

        _ws.AddDocument(WorkspaceDocument.Create(path, content));
    }

    protected string CursorPath => _cursorPositions[^1].Path;
    protected Position1D CursorPosition => _cursorPositions[^1].Position;

    [DebuggerStepThrough]
    protected T Get<T>()
    {
        _sa ??= new SemanticAnalyzer(_ws);
        return _sa.Get<T>();
    }

    protected T Resolve<T>(INodeId<T>? nodeId) where T : SyntaxTreeNode
    {
        if (nodeId == null)
            throw new Exception("NodeId was null");
        return Get<Nodes>().GetNodeById<T>(nodeId);
    }

    protected T Find<T>(Func<T, bool> condition) where T : SyntaxTreeNode
    {
        var result = FindAll(condition);
        return result.Length switch
        {
            0 => throw new Exception("No matching node found"),
            > 1 => throw new Exception("More than one matching node found: " + result.Count()),
            _ => result[0]
        };
    }

    protected ImmutableArray<T> FindAll<T>(Func<T, bool> condition) where T : SyntaxTreeNode
    {
        var result = new List<T>();
        foreach (var codeDoc in _ws.Documents.Values.OfType<CodeWorkspaceDocument>())
        {
            void Traverse(SyntaxTreeNode node)
            {
                if (node is T typedNode && condition(typedNode))
                    result.Add(typedNode);

                foreach (var childNode in node.GetChildNodes())
                    Traverse(childNode);
            }

            Traverse(codeDoc.ParserResult.RootNode);
        }

        return result.ToImmutableArray();
    }

    protected T Find<T>(string tokenText) where T : SyntaxTreeNode
    {
        var allTokenNodeId = FindAllIds(tokenText);
        
        foreach (var tokenNodeId in allTokenNodeId)
        {
            var matchingParent = FindParentOfType<T>(tokenNodeId);
            if (matchingParent != null)
                return matchingParent;
        }

        throw new Exception("Could not find a node of type " + typeof(T).Name + " which contains a token with text '" + tokenText + "'");
    }

    private T? FindParentOfType<T>(INodeId<SyntaxTreeNode> nodeId) where T : SyntaxTreeNode
    {
        var doc = (CodeWorkspaceDocument) _ws.Documents[nodeId.DocumentPath];

        while (true)
        {
            if (!doc.ParserResult.ParentNodeIdsByNodeId.TryGetValue(nodeId, out var parentNodeId))
                throw new Exception("Could not find parent nod id");

            if (parentNodeId == null)
                return null;

            if (parentNodeId is INodeId<T>)
                return (T) doc.ParserResult.NodesByNodeId[parentNodeId];

            nodeId = parentNodeId;
        }
    }

    protected ImmutableArray<TokenNode> FindAll(string tokenText)
    {
        return FindAll<TokenNode>(x => x.Text == tokenText);
    }

    protected TokenNode Find(string tokenText)
    {
        return Find<TokenNode>(x => x.Text == tokenText);
    }

    protected INodeId<TokenNode> FindId(string tokenText)
    {
        return Find(tokenText).NodeId;
    }

    protected ImmutableArray<INodeId<TokenNode>> FindAllIds(string tokenText)
    {
        return FindAll(tokenText).Select(x => x.NodeId).ToImmutableArray();
    }

    protected INodeId<T> FindId<T>(string tokenText) where T : SyntaxTreeNode
    {
        return (INodeId<T>) Find<T>(tokenText).NodeId;
    }
}