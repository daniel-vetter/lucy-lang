﻿using Lucy.Core.Model;
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
        if (result == null)
            return null;
        return result.Node;
    }

    [GenerateDbExtension] ///<see cref="GetRangeFromNodeEx.GetRangeFromNode"/>
    public static Range1D GetRangeFromNode(IDb db, SyntaxTreeNode node)
    {
        var start = db.GetDistanceFromDocumentStart(node);
        var len = db.GetNodeRangeLength(node);
        return new Range1D(new (start), new (start + len));
    }

    [GenerateDbExtension] ///<see cref="GetRangeFromNodeEx.GetRangeFromNodeId"/>
    public static Range1D GetRangeFromNodeId(IDb db, NodeId nodeId)
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
            if (position.Position >= offset && position.Position < + offset + nodeLength)
            {
                return FindNodeFromPosition(db, position, child, offset);
            }
            offset += nodeLength;
        }

        return null;
    }

    private record SearchResult(SyntaxTreeNode Node);

    [GenerateDbExtension] ///<see cref="GetDistanceFromDocumentStartEx.GetDistanceFromDocumentStart"/>
    public static int GetDistanceFromDocumentStart(IDb db, SyntaxTreeNode node)
    {
        if (node.NodeId.IsRoot)
            return 0;

        var parentNode = db.GetNodeById(node.NodeId.Parent);

        var distance = 0;
        foreach(var child in parentNode.GetChildNodes())
        {
            if (child.NodeId == node.NodeId)
                break;
            distance += db.GetNodeRangeLength(child);
        }

        return distance + db.GetDistanceFromDocumentStart(parentNode);
    }

    [GenerateDbExtension] ///<see cref="GetNodeRangeLengthEx.GetNodeRangeLength"/>
    public static int GetNodeRangeLength(IDb db, SyntaxTreeNode node)
    {
        var len = 0;
        foreach(var child in node.GetChildNodes())
            len += db.GetNodeRangeLength(child);

        if (node is TokenNode token)
            len += token.Text.Length;

        return len;
    }
}