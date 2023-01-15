using System;
using System.Collections.Generic;
using System.Linq;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler;

public static class BasicNodeAccess
{
    [DbQuery] ///<see cref="GetNodeByIdEx.GetNodeById"/>
    public static SyntaxTreeNode GetNodeById(IDb db, INodeId<SyntaxTreeNode> nodeId)
    {
        return db.GetNodesByNodeIdMap(nodeId.DocumentPath)[nodeId];
    }

    public static T GetNodeById<T>(this IDb db, INodeId<T> nodeId) where T : SyntaxTreeNode
    {
        return (T)GetNodeByIdEx.GetNodeById(db, nodeId);
    }

    [DbQuery] ///<see cref="GetNodeIdsByTypeEx.GetNodeIdsByType"/>
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
    
    [DbQuery] ///<see cref="GetParentNodeIdEx.GetParentNodeId" />
    public static INodeId<SyntaxTreeNode>? GetParentNodeId(IDb db, INodeId<SyntaxTreeNode> nodeId)
    {
        var map = db.GetParentNodeIdByNodeIdMap(nodeId.DocumentPath);
        if (map.TryGetValue(nodeId, out var parentId) && parentId != null)
            return parentId;
        return null;
    }

    [DbQuery] ///<see cref="GetParentNodeIdOfTypeEx.GetParentNodeIdOfType"/>
    public static INodeId<SyntaxTreeNode>? GetParentNodeIdOfType(IDb db, INodeId<SyntaxTreeNode> nodeId, Type nodeIdType)
    {
        var parentNodeId = db.GetParentNodeId(nodeId);
        if (parentNodeId == null)
            return null;

        return parentNodeId.GetType() == nodeIdType
            ? parentNodeId 
            : db.GetParentNodeIdOfType(parentNodeId, nodeIdType);
    }

    public static INodeId<T>? GetParentNodeIdOfType<T>(this IDb db, INodeId<SyntaxTreeNode> nodeId) where T : SyntaxTreeNode
    {
        return db.GetParentNodeIdOfType(nodeId, typeof(NodeId<T>)) as INodeId<T>;
    }

    [DbQuery] ///<see cref="GetParentNodeIdOfTypesEx.GetParentNodeIdOfTypes"/>
    public static INodeId<SyntaxTreeNode>? GetParentNodeIdOfTypes(IDb db, INodeId<SyntaxTreeNode> nodeId, ComparableReadOnlyList<Type> nodeTypes)
    {
        var parentNode = db.GetParentNodeId(nodeId);
        if (parentNode == null)
            return null;
        
        return nodeTypes.Contains(parentNode.GetType()) 
            ? parentNode
            : db.GetParentNodeIdOfTypes(parentNode, nodeTypes);
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

    [DbQuery] ///<see cref="GetNodeIdsByTypeInStatementListMapEx.GetNodeIdsByTypeInStatementListMap" />
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

    [DbQuery]
    public static ComparableReadOnlyList<INodeId<SyntaxTreeNode>> GetNodeIdsByTypeInStatementList(IDb db, INodeId<StatementListSyntaxNode> nodeId, Type type)
    {
        return db.GetNodeIdsByTypeInStatementListMap(nodeId).TryGetValue(type, out var list) 
            ? list 
            : new ComparableReadOnlyList<INodeId<SyntaxTreeNode>>();
    }

    public static ComparableReadOnlyList<INodeId<T>> GetNodeIdsByTypeInStatementList<T>(this IDb db, INodeId<StatementListSyntaxNode> nodeId) where T : SyntaxTreeNode
    {
        return db.GetNodeIdsByTypeInStatementList(nodeId, typeof(T))
            .Cast<INodeId<T>>()
            .ToComparableReadOnlyList();
    }

    [DbQuery]
    public static ComparableReadOnlyList<INodeId<SyntaxTreeNode>> GetNodeIdsByTypeInStatementListShallow(IDb db, INodeId<StatementListSyntaxNode> nodeId, Type type)
    {
        var result = new ComparableReadOnlyList<INodeId<SyntaxTreeNode>>.Builder();
        foreach (var statement in db.GetNodeById(nodeId).Statements)
        {
            if (statement.GetType() != type)
                continue;

            result.Add(statement.NodeId);
        }
        return result.Build();
    }
    
    public static ComparableReadOnlyList<INodeId<T>> GetNodeIdsByTypeInStatementListShallow<T>(this IDb db, INodeId<StatementListSyntaxNode> nodeId) where T : SyntaxTreeNode
    {
        return db.GetNodeIdsByTypeInStatementListShallow(nodeId, typeof(T))
            .Cast<INodeId<T>>()
            .ToComparableReadOnlyList();
    }
}