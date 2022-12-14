using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler;

public static class BasicNodeAccess
{
    [GenerateDbExtension] ///<see cref="GetNodeByIdMapEx.GetNodeByIdMap"/>
    public static ComparableReadOnlyDictionary<INodeId<SyntaxTreeNode>, SyntaxTreeNode> GetNodeByIdMap(IDb db, string documentPath)
    {
        return db.GetNodeList(db.GetSyntaxTree(documentPath)).ToComparableReadOnlyDictionary(x => x.NodeId, x => x);
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
    
    [GenerateDbExtension] ///<see cref="GetNodeIdsByTypeMapEx.GetNodeIdsByTypeMap"/>
    public static ComparableReadOnlyDictionary<Type, ComparableReadOnlyList<INodeId<SyntaxTreeNode>>> GetNodeIdsByTypeMap(IDb db, string documentPath)
    {
        return db.GetNodeList(db.GetSyntaxTree(documentPath))
            .GroupBy(x => x.GetType())
            .ToComparableReadOnlyDictionary(x => x.Key, x => x.Select(y => y.NodeId).ToComparableReadOnlyList());
    }

    [GenerateDbExtension] ///<see cref="GetNodeIdsByTypeEx.GetNodeIdsByType"/>
    public static ComparableReadOnlyList<INodeId<SyntaxTreeNode>> GetNodeIdsByType(IDb db, string documentPath, Type type)
    {
        return db.GetNodeIdsByTypeMap(documentPath).TryGetValue(type, out var list) 
            ? list.ToComparableReadOnlyList() 
            : new ComparableReadOnlyList<INodeId<SyntaxTreeNode>>();
    }

    public static ComparableReadOnlyList<INodeId<T>> GetNodeIdsByType<T>(this IDb db, string documentPath) where T : SyntaxTreeNode
    {
        return db.GetNodeIdsByType(documentPath, typeof(T))
            .Cast<INodeId<T>>()
            .ToComparableReadOnlyList();
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

    [GenerateDbExtension]
    public static ComparableReadOnlyDictionary<INodeId<SyntaxTreeNode>, INodeId<SyntaxTreeNode>?> GetParentMap(IDb db, string documentPath)
    {
        var dict = new ComparableReadOnlyDictionary<INodeId<SyntaxTreeNode>, INodeId<SyntaxTreeNode>?>.Builder();

        void Traverse(SyntaxTreeNode parent)
        {
            foreach (var child in parent.GetChildNodes())
            {
                dict.Add(child.NodeId, parent.NodeId);
                Traverse(child);
            }
        }

        var root = db.GetSyntaxTree(documentPath);
        Traverse(root);
        dict.Add(root.NodeId, null);

        return dict.Build();
    }

    [GenerateDbExtension] ///<see cref="GetParentNodeEx.GetParentNode" />
    public static SyntaxTreeNode? GetParentNode(IDb db, INodeId<SyntaxTreeNode> nodeId)
    {
        var map = db.GetParentMap(nodeId.DocumentPath);
        if (map.TryGetValue(nodeId, out var parentId) && parentId != null)
            return db.GetNodeById(parentId);
        return null;
     /*   
        var lastIndex = nodeId.NodePath.LastIndexOf('.');
        if (lastIndex == -1)
            return null;
        var parentNodeId = new NodeId<SyntaxTreeNode, SyntaxTreeNodeBuilder>(nodeId.DocumentPath, nodeId.NodePath[..lastIndex]);

        return db.GetNodeById(parentNodeId);
       */ 
    }

    [GenerateDbExtension] ///<see cref="GetParentNodeIdOfTypeEx.GetParentNodeIdOfType"/>
    public static INodeId<SyntaxTreeNode>? GetParentNodeIdOfType(IDb db, INodeId<SyntaxTreeNode> nodeId, Type nodeType)
    {
        var parentNode = db.GetParentNode(nodeId);
        if (parentNode == null)
            return null;

        return parentNode.GetType() == nodeType 
            ? parentNode.NodeId 
            : db.GetParentNodeIdOfType(parentNode.NodeId, nodeType);
    }

    public static INodeId<T>? GetParentNodeIdOfType<T>(this IDb db, INodeId<SyntaxTreeNode> nodeId) where T : SyntaxTreeNode
    {
        return db.GetParentNodeIdOfType(nodeId, typeof(T)) as INodeId<T>;
    }

    [GenerateDbExtension] ///<see cref="GetParentNodeIdOfTypesEx.GetParentNodeIdOfTypes"/>
    public static INodeId<SyntaxTreeNode>? GetParentNodeIdOfTypes(IDb db, INodeId<SyntaxTreeNode> nodeId, ComparableReadOnlyList<Type> nodeTypes)
    {
        var parentNode = db.GetParentNode(nodeId);
        if (parentNode == null)
            return null;
        
        return nodeTypes.Contains(parentNode.GetType()) 
            ? parentNode.NodeId 
            : db.GetParentNodeIdOfTypes(parentNode.NodeId, nodeTypes);
    }

    public static INodeId<SyntaxTreeNode>? GetParentNodeIdOfTypes<T1, T2>(this IDb db, INodeId<SyntaxTreeNode> nodeId) 
        where T1 : SyntaxTreeNode
        where T2 : SyntaxTreeNode
    {
        return db.GetParentNodeIdOfTypes(nodeId, new ComparableReadOnlyList<Type>(new[] { typeof(T1), typeof(T2) }));
    }

    public static INodeId<SyntaxTreeNode>? GetParentNodeIdOfTypes<T1, T2, T3>(this IDb db, INodeId<SyntaxTreeNode> nodeId)
        where T1 : SyntaxTreeNode
        where T2 : SyntaxTreeNode
        where T3 : SyntaxTreeNode
    {
        return db.GetParentNodeIdOfTypes(nodeId, new ComparableReadOnlyList<Type>(new[] { typeof(T1), typeof(T2), typeof(T3) }));
    }

    [GenerateDbExtension] ///<see cref="GetNodeIdsByTypeInStatementListMapEx.GetNodeIdsByTypeInStatementListMap" />
    public static ComparableReadOnlyDictionary<Type, ComparableReadOnlyList<INodeId<SyntaxTreeNode>>> GetNodeIdsByTypeInStatementListMap(IDb db, INodeId<StatementListSyntaxNode> nodeId)
    {
        var currentNode = db.GetNodeById(nodeId);
        var list = new List<SyntaxTreeNode>();

        void Traverse(SyntaxTreeNode node)
        {
            foreach (var childNode in node.GetChildNodes())
            {
                list.Add(childNode);
                if (childNode is not StatementListSyntaxNode)
                    Traverse(childNode);
            }
        }

        list.Add(currentNode);
        Traverse(currentNode);
        return list
            .GroupBy(x => x.GetType())
            .ToComparableReadOnlyDictionary(x => x.Key, x => x.Select(y => y.NodeId).ToComparableReadOnlyList());
    }

    [GenerateDbExtension] ///<see cref="GetNodeIdsByTypeInStatementListEx.GetNodeIdsByTypeInStatementList" />
    public static ComparableReadOnlyList<INodeId<SyntaxTreeNode>> GetNodeIdsByTypeInStatementList(IDb db, INodeId<StatementListSyntaxNode> nodeId, Type type)
    {
        return db.GetNodeIdsByTypeInStatementListMap(nodeId).TryGetValue(type, out var list) 
            ? list 
            : new ComparableReadOnlyList<INodeId<SyntaxTreeNode>>();
    }

    public static ComparableReadOnlyList<INodeId<T>> GetNodeIdsByTypeInStatementList<T>(this IDb db, INodeId<StatementListSyntaxNode> nodeId) where T: SyntaxTreeNode
    {
        return db.GetNodeIdsByTypeInStatementList(nodeId, typeof(T))
            .Cast<INodeId<T>>()
            .ToComparableReadOnlyList();
    }
}