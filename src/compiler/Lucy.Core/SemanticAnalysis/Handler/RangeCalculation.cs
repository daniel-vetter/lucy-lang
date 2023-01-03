using Lucy.Core.Model;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler;

public static class RangeCalculation
{
    [GenerateDbExtension] ///<see cref="GetNodeAtPositionEx.GetNodeAtPosition"/>
    public static SyntaxTreeNode? GetNodeAtPosition(IDb db, string documentPath, Position1D position)
    {
        var root = db.GetSyntaxTree(documentPath);
        var result = FindNodeFromPosition(db, position, root, 0);
        return result?.Node;
    }

    [GenerateDbExtension] ///<see cref="GetRangeFromNodeEx.GetRangeFromNode"/>
    public static Range1D GetRangeFromNode(IDb db, SyntaxTreeNode node)
    {
        var start = db.GetDistanceFromDocumentStart(node);
        var len = db.GetNodeRangeLength(node);
        return new Range1D(start, start + len.WithoutTrailingTrivia);
    }

    [GenerateDbExtension] ///<see cref="GetRangeFromNodeEx.GetRangeFromNodeId"/>
    public static Range1D GetRangeFromNodeId(IDb db, INodeId<SyntaxTreeNode> nodeId)
    {
        return db.GetRangeFromNode(db.GetNodeById(nodeId));
    }

    private static SearchResult? FindNodeFromPosition(IDb db, Position1D position, SyntaxTreeNode startNode, int offset)
    {
        if (!startNode.GetChildNodes().Any())
        {
            return new SearchResult(startNode);
        }
            
        foreach (var child in startNode.GetChildNodes())
        {
            var nodeLength = db.GetNodeRangeLength(child);
            if (position.Position >= offset && position.Position < + offset + nodeLength.WithoutTrailingTrivia)
            {
                return FindNodeFromPosition(db, position, child, offset);
            }
            offset += nodeLength.WithTrailingTrivia;
        }

        return null;
    }

    private record SearchResult(SyntaxTreeNode Node);

    [GenerateDbExtension] ///<see cref="GetDistanceFromDocumentStartEx.GetDistanceFromDocumentStart"/>
    public static int GetDistanceFromDocumentStart(IDb db, SyntaxTreeNode node)
    {
        var distance = 0;
        var current = node;

        while (current != null)
        {
            distance += db.GetDistanceFromParent(current);
            current = db.GetParentNode(current.NodeId);
        }

        return distance;
    }

    [GenerateDbExtension] ///<see cref="GetDistanceFromDocumentStartEx.GetDistanceFromParent"/>
    public static int GetDistanceFromParent(IDb db, SyntaxTreeNode node)
    {
        var parentNode = db.GetParentNode(node.NodeId);
        if (parentNode == null)
            return 0;

        var distance = 0;
        foreach (var child in parentNode.GetChildNodes())
        {
            if (child.NodeId == node.NodeId)
                break;
            distance += db.GetNodeRangeLength(child).WithTrailingTrivia;
        }

        return distance;
    }

    [GenerateDbExtension] ///<see cref="GetNodeRangeLengthEx.GetNodeRangeLength"/>
    public static RangeResult GetNodeRangeLength(IDb db, SyntaxTreeNode node)
    {
        var len = new RangeResult(0, 0);
        foreach (var child in node.GetChildNodes())
        {
            var result = db.GetNodeRangeLength(child);
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