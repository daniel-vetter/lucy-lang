using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler;

public static class GetNodeMapHandler
{
    [GenerateDbExtension] ///<see cref="GetNodeByIdMapEx.GetNodeByIdMap"/>
    public static ComparableReadOnlyDictionary<NodeId, SyntaxTreeNode> GetNodeByIdMap(IDb db, string documentPath)
    {
        return db.GetNodeList(db.GetSyntaxTree(documentPath)).ToComparableReadOnlyDictionary(x => x.NodeId, x => x);
    }

    [GenerateDbExtension] ///<see cref="GetNodeTypeByIdEx.GetNodeTypeById"/>
    public static Type GetNodeTypeById(IDb db, NodeId nodeId)
    {
        return db.GetNodeByIdMap(nodeId.DocumentPath)[nodeId].GetType();
    }

    [GenerateDbExtension] ///<see cref="GetNodeByIdEx.GetNodeById"/>
    public static SyntaxTreeNode GetNodeById(IDb db, NodeId nodeId)
    {
        return db.GetNodeByIdMap(nodeId.DocumentPath)[nodeId];
    }

    [GenerateDbExtension] ///<see cref="GetNodesByTypeMapEx.GetNodesByTypeMap"/>
    public static ComparableReadOnlyDictionary<Type, ComparableReadOnlyList<NodeId>> GetNodesByTypeMap(IDb db, string documentPath)
    {
        return db.GetNodeList(db.GetSyntaxTree(documentPath))
            .GroupBy(x => x.GetType())
            .ToComparableReadOnlyDictionary(x => x.Key, x => x.Select(y => y.NodeId).ToComparableReadOnlyList());
    }
    /*
    [GenerateDbExtension] ///<see cref="GetNodesByTypeEx.GetNodesByType"/>
    public static ComparableReadOnlyList<SyntaxTreeNode> GetNodesByType(IDb db, string documentPath, Type type)
    {
        if (db.GetNodesByTypeMap(documentPath).TryGetValue(type, out var list))
            return list;
        return new ComparableReadOnlyList<SyntaxTreeNode>();
    }*/

    [GenerateDbExtension] ///<see cref="GetNodesByTypeEx.GetNodesByType"/>
    public static ComparableReadOnlyList<NodeId> GetNodeIdsByType(IDb db, string documentPath, Type type)
    {
        if (db.GetNodesByTypeMap(documentPath).TryGetValue(type, out var list))
            return list.ToComparableReadOnlyList();
        return new ComparableReadOnlyList<NodeId>();
    }

    public static ComparableReadOnlyList<NodeId> GetNodeIdsByType<T>(this IDb db, string documentPath) where T : SyntaxTreeNode
    {
        return db.GetNodeIdsByType(documentPath, typeof(T));
    }

    /*
    public static ComparableReadOnlyList<T> GetNodesByType<T>(this IDb db, string documentPath) where T : SyntaxTreeNode
    {
        return db.GetNodesByType(documentPath, typeof(T)).Cast<T>().ToComparableReadOnlyList();
    }*/

    public static ComparableReadOnlyList<NodeId> GetNodeIdByType<T>(this IDb db, string documentPath) where T : SyntaxTreeNode
    {
        return db.GetNodeIdsByType(documentPath, typeof(T));
    }

    [GenerateDbExtension] ///<see cref="GetNodeListEx.GetNodeList"/>
    public static ComparableReadOnlyList<SyntaxTreeNode> GetNodeList(IDb db, SyntaxTreeNode node)
    {
        static void Traverse(IDb db, SyntaxTreeNode node, ComparableReadOnlyList<SyntaxTreeNode>.Builder nodes)
        {
            nodes.Add(node);

            if (node is StatementListSyntaxNode)
            {
                foreach (var child in node.GetChildNodes())
                    nodes.AddRange(db.GetNodeList(child));
            }
            else
            {
                foreach (var child in node.GetChildNodes())
                    Traverse(db, child, nodes);
            }
        }

        var list = new ComparableReadOnlyList<SyntaxTreeNode>.Builder();
        Traverse(db, node, list);
        return list.Build();
    }

    [GenerateDbExtension] ///<see cref="GetNearestParentNodeOfTypeEx.GetNearestParentNodeOfType"/>
    public static NodeId? GetNearestParentNodeOfType(IDb db, NodeId nodeId, Type nodeType)
    {
        if (nodeId.IsRoot)
            return null;

        var parentId = nodeId.Parent;
        if (nodeType == db.GetNodeTypeById(parentId))
            return parentId;

        return db.GetNearestParentNodeOfType(parentId, nodeType);
    }

    public static NodeId? GetNearestParentNodeOfType<T>(this IDb db, NodeId nodeId)
    {
        var parentNodeId = db.GetNearestParentNodeOfType(nodeId, typeof(T));
        if (parentNodeId == null) 
            return null;
        return parentNodeId;
    }
}