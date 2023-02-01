using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler;

[QueryGroup]
public class Nodes
{
    private readonly SemanticAnalysisInput _input;

    public Nodes(SemanticAnalysisInput input)
    {
        _input = input;
    }

    public virtual ImmutableHashSet<string> GetDocumentList()
    {
        return _input.ParsedCodeFiles.Keys.ToImmutableHashSet();
    }

    public virtual DocumentRootSyntaxNode GetSyntaxTree(string documentPath)
    {
        return _input.ParsedCodeFiles[documentPath].RootNode;
    }

    protected virtual ImmutableDictionary<INodeId<SyntaxTreeNode>, SyntaxTreeNode> GetNodesByNodeIdMap(string documentPath)
    {
        return _input.ParsedCodeFiles[documentPath].NodesByNodeId;
    }

    
    public T GetNodeById<T>(INodeId<T> nodeId) where T : SyntaxTreeNode
    {
        return (T) GetNodeByIdUntyped(nodeId);
    }
    
    protected virtual SyntaxTreeNode GetNodeByIdUntyped(INodeId<SyntaxTreeNode> nodeId)
    {
        return GetNodesByNodeIdMap(nodeId.DocumentPath)[nodeId];
    }
    
    protected virtual ImmutableDictionary<Type, ImmutableHashSet<INodeId<SyntaxTreeNode>>> GetNodeIdsByTypeMap(string documentPath)
    {
        return _input.ParsedCodeFiles[documentPath].NodeIdsByType;
    }

    protected virtual ComparableReadOnlyList<INodeId<SyntaxTreeNode>> GetNodeIdsByTypeUntyped(string documentPath, Type type)
    {
        return GetNodeIdsByTypeMap(documentPath).TryGetValue(type, out var list)
            ? list.ToComparableReadOnlyList()
            : new ComparableReadOnlyList<INodeId<SyntaxTreeNode>>();
    }

    public ComparableReadOnlyList<INodeId<T>> GetNodeIdsByType<T>(string documentPath) where T : SyntaxTreeNode
    {
        return GetNodeIdsByTypeUntyped(documentPath, typeof(T))
            .Cast<INodeId<T>>()
            .ToComparableReadOnlyList();
    }

    protected virtual ImmutableDictionary<INodeId<SyntaxTreeNode>, INodeId<SyntaxTreeNode>?> GetParentNodeIdByNodeIdMap(string documentPath)
    {
        return _input.ParsedCodeFiles[documentPath].ParentNodeIdByNodeIds;
    }

    public virtual INodeId<SyntaxTreeNode>? GetParentNodeId(INodeId<SyntaxTreeNode> nodeId)
    {
        var map = GetParentNodeIdByNodeIdMap(nodeId.DocumentPath);
        if (map.TryGetValue(nodeId, out var parentId) && parentId != null)
            return parentId;
        return null;
    }

    protected virtual INodeId<SyntaxTreeNode>? GetParentNodeIdOfTypeUntyped(INodeId<SyntaxTreeNode> nodeId, Type nodeIdType)
    {
        var parentNodeId = GetParentNodeId(nodeId);
        if (parentNodeId == null)
            return null;

        return parentNodeId.GetType() == nodeIdType
            ? parentNodeId
            : GetParentNodeIdOfTypeUntyped(parentNodeId, nodeIdType);
    }

    public INodeId<T>? GetParentNodeIdOfType<T>(INodeId<SyntaxTreeNode> nodeId) where T : SyntaxTreeNode
    {
        return GetParentNodeIdOfTypeUntyped(nodeId, typeof(NodeId<T>)) as INodeId<T>;
    }

    protected virtual INodeId<SyntaxTreeNode>? GetParentNodeIdOfTypes(INodeId<SyntaxTreeNode> nodeId, ComparableReadOnlyList<Type> nodeTypes)
    {
        var parentNode = GetParentNodeId(nodeId);
        if (parentNode == null)
            return null;

        return nodeTypes.Contains(parentNode.GetType())
            ? parentNode
            : GetParentNodeIdOfTypes(parentNode, nodeTypes);
    }

    public INodeId<SyntaxTreeNode>? GetParentNodeIdOfTypes<T1, T2>(INodeId<SyntaxTreeNode> nodeId)
        where T1 : SyntaxTreeNode
        where T2 : SyntaxTreeNode
    {
        return GetParentNodeIdOfTypes(nodeId, new ComparableReadOnlyList<Type>(new[] {typeof(T1), typeof(T2)}));
    }

    public INodeId<SyntaxTreeNode>? GetParentNodeIdOfTypes<T1, T2, T3>(INodeId<SyntaxTreeNode> nodeId)
        where T1 : SyntaxTreeNode
        where T2 : SyntaxTreeNode
        where T3 : SyntaxTreeNode
    {
        return GetParentNodeIdOfTypes(nodeId, new ComparableReadOnlyList<Type>(new[] {typeof(T1), typeof(T2), typeof(T3)}));
    }

    protected virtual ComparableReadOnlyDictionary<Type, ComparableReadOnlyList<INodeId<SyntaxTreeNode>>> GetNodeIdsByTypeInStatementListMap(
        INodeId<StatementListSyntaxNode> nodeId)
    {
        var currentNode = GetNodeById(nodeId);
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

    protected virtual ComparableReadOnlyList<INodeId<SyntaxTreeNode>> GetNodeIdsByTypeInStatementList(INodeId<StatementListSyntaxNode> nodeId, Type type)
    {
        return GetNodeIdsByTypeInStatementListMap(nodeId).TryGetValue(type, out var list)
            ? list
            : new ComparableReadOnlyList<INodeId<SyntaxTreeNode>>();
    }

    public ComparableReadOnlyList<INodeId<T>> GetNodeIdsByTypeInStatementList<T>(INodeId<StatementListSyntaxNode> nodeId) where T : SyntaxTreeNode
    {
        return GetNodeIdsByTypeInStatementList(nodeId, typeof(T))
            .Cast<INodeId<T>>()
            .ToComparableReadOnlyList();
    }

    protected virtual ComparableReadOnlyList<INodeId<SyntaxTreeNode>> GetNodeIdsByTypeInStatementListShallow(INodeId<StatementListSyntaxNode> nodeId, Type type)
    {
        var result = new ComparableReadOnlyList<INodeId<SyntaxTreeNode>>.Builder();
        foreach (var statement in GetNodeById(nodeId).Statements)
        {
            if (statement.GetType() != type)
                continue;

            result.Add(statement.NodeId);
        }

        return result.Build();
    }

    public ComparableReadOnlyList<INodeId<T>> GetNodeIdsByTypeInStatementListShallow<T>(INodeId<StatementListSyntaxNode> nodeId)
        where T : SyntaxTreeNode
    {
        return GetNodeIdsByTypeInStatementListShallow(nodeId, typeof(T))
            .Cast<INodeId<T>>()
            .ToComparableReadOnlyList();
    }
}