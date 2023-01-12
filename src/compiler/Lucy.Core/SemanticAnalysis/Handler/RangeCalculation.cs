using System.Linq;
using Lucy.Core.Model;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler;

public static class RangeCalculation
{
    [DbQuery] ///<see cref="GetNodeAtPositionEx.GetNodeAtPosition"/>
    public static INodeId<SyntaxTreeNode>? GetNodeAtPosition(IDb db, string documentPath, Position1D position)
    {
        static SearchResult? Find(IDb db, Position1D position, SyntaxTreeNode startNode, int offset)
        {
            if (!startNode.GetChildNodes().Any())
                return new SearchResult(startNode);

            foreach (var child in startNode.GetChildNodes())
            {
                var nodeLength = db.GetNodeRangeLength(child.NodeId);
                if (position.Position >= offset && position.Position < +offset + nodeLength.WithTrailingTrivia)
                    return Find(db, position, child, offset);
                offset += nodeLength.WithTrailingTrivia;
            }

            return null;
        }

        var root = db.GetSyntaxTree(documentPath);
        var result = Find(db, position, root, 0);
        return result?.Node.NodeId;
    }
    
    [DbQuery] ///<see cref="GetRangeFromNodeEx.GetRangeFromNodeId"/>
    public static Range1D GetRangeFromNodeId(IDb db, INodeId<SyntaxTreeNode> nodeId)
    {
        var start = db.GetDistanceFromDocumentStart(nodeId);
        var len = db.GetNodeRangeLength(nodeId);
        return new Range1D(start, start + len.WithoutTrailingTrivia);
    }
    
    private record SearchResult(SyntaxTreeNode Node);

    [DbQuery] ///<see cref="GetDistanceFromDocumentStartEx.GetDistanceFromDocumentStart"/>
    public static int GetDistanceFromDocumentStart(IDb db, INodeId<SyntaxTreeNode> nodeId)
    {
        var distance = 0;
        var current = nodeId;

        while (current != null)
        {
            distance += db.GetDistanceFromParent(current);
            current = db.GetParentNodeId(current);
        }

        return distance;
    }

    [DbQuery] ///<see cref="GetDistanceFromDocumentStartEx.GetDistanceFromParent"/>
    public static int GetDistanceFromParent(IDb db, INodeId<SyntaxTreeNode> nodeId)
    {
        var parentNodeId = db.GetParentNodeId(nodeId);
        if (parentNodeId == null)
            return 0;

        var distance = 0;
        foreach (var child in db.GetNodeById(parentNodeId).GetChildNodes())
        {
            if (child.NodeId == nodeId)
                break;
            distance += db.GetNodeRangeLength(child.NodeId).WithTrailingTrivia;
        }

        return distance;
    }

    [DbQuery] ///<see cref="GetNodeRangeLengthEx.GetNodeRangeLength"/>
    public static RangeResult GetNodeRangeLength(IDb db, INodeId<SyntaxTreeNode> nodeId)
    {
        var len = new RangeResult(0, 0);
        var node = db.GetNodeById(nodeId);
        foreach (var child in node.GetChildNodes())
        {
            var result = db.GetNodeRangeLength(child.NodeId);
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