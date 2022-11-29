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
    public static ComparableReadOnlyDictionary<INodeId<SyntaxTreeNode>, SyntaxTreeNode> GetNodeByIdMap(IDb db, string documentPath)
    {
        return db.GetNodeList(db.GetSyntaxTree(documentPath)).ToComparableReadOnlyDictionary(x => x.NodeId, x => x);
    }

    [GenerateDbExtension] ///<see cref="GetNodeTypeByIdEx.GetNodeTypeById"/>
    public static Type GetNodeTypeById(IDb db, INodeId<SyntaxTreeNode> nodeId)
    {
        return db.GetNodeByIdMap(nodeId.DocumentPath)[nodeId].GetType();
    }

    [GenerateDbExtension] ///<see cref="GetNodeByIdEx.GetNodeById"/>
    public static SyntaxTreeNode GetNodeById(IDb db, INodeId<SyntaxTreeNode> nodeId)
    {
        return db.GetNodeByIdMap(nodeId.DocumentPath)[nodeId];
    }

    public static T GetNodeById<T>(this IDb db, INodeId<T> nodeId) where T : SyntaxTreeNode
    {
        return (T)GetNodeByIdEx.GetNodeById(db, nodeId);
    }

    [GenerateDbExtension] ///<see cref="GetNodesByTypeMapEx.GetNodesByTypeMap"/>
    public static ComparableReadOnlyDictionary<Type, ComparableReadOnlyList<INodeId<SyntaxTreeNode>>> GetNodesByTypeMap(IDb db, string documentPath)
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
    public static ComparableReadOnlyList<INodeId<SyntaxTreeNode>> GetNodeIdsByType(IDb db, string documentPath, Type type)
    {
        return db.GetNodesByTypeMap(documentPath).TryGetValue(type, out var list) 
            ? list.ToComparableReadOnlyList() 
            : new ComparableReadOnlyList<INodeId<SyntaxTreeNode>>();
    }

    public static ComparableReadOnlyList<INodeId<T>> GetNodeIdsByType<T>(this IDb db, string documentPath) where T : SyntaxTreeNode
    {
        return db.GetNodeIdsByType(documentPath, typeof(T))
            .Cast<INodeId<T>>()
            .ToComparableReadOnlyList();
    }

    /*
    public static ComparableReadOnlyList<T> GetNodesByType<T>(this IDb db, string documentPath) where T : SyntaxTreeNode
    {
        return db.GetNodesByType(documentPath, typeof(T)).Cast<T>().ToComparableReadOnlyList();
    }*/
    
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
    public static INodeId<SyntaxTreeNode>? GetNearestParentNodeOfType(IDb db, INodeId<SyntaxTreeNode> nodeId, Type nodeType)
    {
        if (nodeId.IsRoot)
            return null;

        var parentId = nodeId.Parent;

        return nodeType == db.GetNodeTypeById(parentId) 
            ? parentId 
            : db.GetNearestParentNodeOfType(parentId, nodeType);
    }

    public static INodeId<T>? GetNearestParentNodeOfType<T>(this IDb db, INodeId<SyntaxTreeNode> nodeId) where T : SyntaxTreeNode
    {
        return db.GetNearestParentNodeOfType(nodeId, typeof(T)) as INodeId<T>;
    }
}