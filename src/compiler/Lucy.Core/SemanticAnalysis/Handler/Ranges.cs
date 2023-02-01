using System.Linq;
using Lucy.Core.Model;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler;

[QueryGroup]
public class Ranges
{
    private readonly Nodes _nodes;

    public Ranges(Nodes nodes)
    {
        _nodes = nodes;
    }
    
    public virtual INodeId<SyntaxTreeNode>? GetNodeAtPosition(string documentPath, Position1D position)
    {
        SearchResult? Find(Position1D position, SyntaxTreeNode startNode, int offset)
        {
            if (!startNode.GetChildNodes().Any())
                return new SearchResult(startNode);

            foreach (var child in startNode.GetChildNodes())
            {
                var nodeLength = GetNodeRangeLength(child.NodeId);
                if (position.Position >= offset && position.Position < +offset + nodeLength.WithTrailingTrivia)
                    return Find(position, child, offset);
                offset += nodeLength.WithTrailingTrivia;
            }

            return null;
        }

        var root = _nodes.GetSyntaxTree(documentPath);
        var result = Find(position, root, 0);
        return result?.Node.NodeId;
    }
    
    public virtual Range1D GetRangeFromNodeId(INodeId<SyntaxTreeNode> nodeId)
    {
        var start = GetDistanceFromDocumentStart(nodeId);
        var len = GetNodeRangeLength(nodeId);
        return new Range1D(start, start + len.WithoutTrailingTrivia);
    }
    
    private record SearchResult(SyntaxTreeNode Node);

    protected virtual int GetDistanceFromDocumentStart(INodeId<SyntaxTreeNode> nodeId)
    {
        var distance = 0;
        var current = nodeId;

        while (current != null)
        {
            distance += GetDistanceFromParent(current);
            current = _nodes.GetParentNodeId(current);
        }

        return distance;
    }

    protected virtual int GetDistanceFromParent(INodeId<SyntaxTreeNode> nodeId)
    {
        var parentNodeId = _nodes.GetParentNodeId(nodeId);
        if (parentNodeId == null)
            return 0;

        var distance = 0;
        foreach (var child in _nodes.GetNodeById(parentNodeId).GetChildNodes())
        {
            if (child.NodeId == nodeId)
                break;
            distance += GetNodeRangeLength(child.NodeId).WithTrailingTrivia;
        }

        return distance;
    }

    protected virtual RangeResult GetNodeRangeLength(INodeId<SyntaxTreeNode> nodeId)
    {
        var len = new RangeResult(0, 0);
        var node = _nodes.GetNodeById(nodeId);
        foreach (var child in node.GetChildNodes())
        {
            var result = GetNodeRangeLength(child.NodeId);
            len = new RangeResult(
                len.WithTrailingTrivia + result.WithTrailingTrivia,
                len.WithTrailingTrivia + result.WithoutTrailingTrivia
            );
        }

        if (node is TokenNode token)
        {
            len = new RangeResult(
                len.WithTrailingTrivia + token.Text.Length + (token.TrailingTrivia?.Length ?? 0),
                len.WithoutTrailingTrivia + token.Text.Length
            );
        }

        return len;
    }
}

public record RangeResult(int WithTrailingTrivia, int WithoutTrailingTrivia);