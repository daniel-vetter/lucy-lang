using System;
using System.Collections.Generic;
using Lucy.Common;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.ProjectManagement;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Lucy.Core.Parsing;

//TODO: General refactoring of this file

public static class Parser
{
    public static ParserResult Parse(string documentPath, string content)
    {
        Profiler.Start("Parsing " + documentPath);
        Profiler.Start("Creating syntax tree");
        var reader = new Reader(documentPath, content);
        var rootNode = DocumentRootSyntaxNodeParser.ReadDocumentSyntaxNode(reader);
        Profiler.End("Creating syntax tree");

        Profiler.Start("Assigning node ids");
        IdAssigner.AssignNewIds(documentPath, rootNode);
        Profiler.End("Assigning node ids");

        Profiler.Start("Building id maps");
        var nodesById = ImmutableDictionary.CreateBuilder<INodeId<SyntaxTreeNode>, SyntaxTreeNode>();
        var parentNodeIdsByNodeId = ImmutableDictionary.CreateBuilder<INodeId<SyntaxTreeNode>, INodeId<SyntaxTreeNode>?>();
        var nodeIdsByType = new Dictionary<Type, ImmutableHashSet<INodeId<SyntaxTreeNode>>.Builder>();

        void Traverse(SyntaxTreeNode? parent, SyntaxTreeNode node)
        {
            nodesById.Add(node.NodeId, node);
            parentNodeIdsByNodeId.Add(node.NodeId, parent?.NodeId);

            var type = node.GetType();
            if (nodeIdsByType.TryGetValue(type, out var list))
                list.Add(node.NodeId);
            else
            {
                var builder = ImmutableHashSet.CreateBuilder<INodeId<SyntaxTreeNode>>();
                builder.Add(node.NodeId);
                nodeIdsByType.Add(type, builder);
            }

            foreach (var childNode in node.GetChildNodes())
                Traverse(node, childNode);
        }

        Traverse(null, rootNode);

        var result = new ParserResult(
            Reader: reader,
            RootNode: rootNode,
            NodesById: nodesById.ToImmutable(),
            ParentNodeIdsByNodeId: parentNodeIdsByNodeId.ToImmutable(),
            NodeIdsByType: nodeIdsByType.ToImmutableDictionary(x => x.Key, x => x.Value.ToImmutable())
        );

        Profiler.End("Building id maps");
        Profiler.End("Parsing " + documentPath);
        return result;
    }

    public static ParserResult Update(ParserResult lastResult, Range1D range, string newContent)
    {
        Profiler.Start("Reparsing " + lastResult.RootNode.NodeId.DocumentPath);
        Profiler.Start("Updating syntax tree");
        var newReader = lastResult.Reader.Update(range, newContent, out var removedCachedEntries);
        var newRootNode = DocumentRootSyntaxNodeParser.ReadDocumentSyntaxNode(newReader);
        Profiler.End("Updating syntax tree");

        Profiler.Start("Updating node ids");
        IdAssigner.ReassignIdsFromPreviousTreeOrCreateNewOnes(
            oldTree: lastResult.RootNode,
            newTree: newRootNode,
            candidates: removedCachedEntries.OfType<SyntaxTreeNode>().ToImmutableArray()
        );
        Profiler.End("Updating node ids");

        Profiler.Start("Updating id maps");
        var diffs = TreeDiffer.Diff(lastResult.RootNode, newRootNode);

        var nodesById = lastResult.NodesById;
        var parentNodeIdsByNodeId = lastResult.ParentNodeIdsByNodeId;
        var nodeIdsByType = lastResult.NodeIdsByType;

        foreach (var detachment in diffs.OfType<NodeDetached>())
        {
            var stack = new Stack<SyntaxTreeNode>();
            stack.Push(detachment.Node);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                nodesById = nodesById.Remove(node.NodeId);
                parentNodeIdsByNodeId = parentNodeIdsByNodeId.Remove(node.NodeId);
                if (!nodeIdsByType.TryGetValue(node.GetType(), out var list))
                    throw new Exception("Could not find node id list for type: " + node.GetType());

                list = list.Remove(node.NodeId);
                nodeIdsByType = nodeIdsByType.SetItem(node.GetType(), list);

                foreach (var child in node.GetChildNodes())
                    stack.Push(child);
            }
        }

        foreach (var attachment in diffs.OfType<NodeAttached>())
        {
            var stack = new Stack<(SyntaxTreeNode? Parent, SyntaxTreeNode Node)>();
            stack.Push((attachment.Parent, attachment.Node));
            while (stack.Count > 0)
            {
                var (parent, node) = stack.Pop();
                nodesById = nodesById.Add(node.NodeId, node);
                parentNodeIdsByNodeId = parentNodeIdsByNodeId.Add(node.NodeId, parent?.NodeId);
                if (!nodeIdsByType.TryGetValue(node.GetType(), out var list))
                    nodeIdsByType = nodeIdsByType.Add(node.GetType(), ImmutableHashSet.Create(node.NodeId));
                else
                {
                    list = list.Add(node.NodeId);
                    nodeIdsByType = nodeIdsByType.SetItem(node.GetType(), list);
                }

                foreach (var child in node.GetChildNodes())
                    stack.Push((node, child));
            }
        }

        foreach (var replacement in diffs.OfType<NodeReplaced>())
        {
            nodesById = nodesById.SetItem(replacement.NewNode.NodeId, replacement.NewNode);
        }

        Profiler.End("Updating id maps");
        Profiler.End("Reparsing " + lastResult.RootNode.NodeId.DocumentPath);

        var result = new ParserResult(
            Reader: newReader,
            RootNode: newRootNode,
            NodesById: nodesById,
            ParentNodeIdsByNodeId: parentNodeIdsByNodeId,
            NodeIdsByType: nodeIdsByType
        );

        //ValidateParserResult(result);

        return result;
    }

    [Conditional("DEBUG")]
    private static void ValidateParserResult(ParserResult result)
    {
        Profiler.Start("Validating result");
        try
        {
            List<(SyntaxTreeNode? Parent, SyntaxTreeNode Node)> allNodes = new();

            void Traverse(SyntaxTreeNode? parent, SyntaxTreeNode node)
            {
                allNodes.Add((parent, node));
                foreach (var child in node.GetChildNodes())
                    Traverse(node, child);
            }
            Traverse(null, result.RootNode);

            if (allNodes.Any(x => x.Node.NodeId.IsMissing()))
                throw new Exception("Syntax tree has missing nodes");

            if (allNodes.GroupBy(x => x.Node.NodeId).Any(x => x.Count() > 1))
                throw new Exception("Syntax tree contains Id duplicates");

            if (allNodes.Select(x => x.Node.NodeId).Except(result.NodesById.Keys).Any())
                throw new Exception("NodeByIdMap is missing node ids");

            var nodeIdToNodeMismatches = allNodes.Where(x => !ReferenceEquals(x.Node, result.NodesById[x.Node.NodeId])).ToArray();
            if (nodeIdToNodeMismatches.Length > 0)
                throw new Exception($"NodeByIdMap: {nodeIdToNodeMismatches.Length} Nodes do not match to node id");

            if (result.NodesById.Keys.Except(allNodes.Select(x => x.Node.NodeId)).Any())
                throw new Exception("NodeByIdMap has node ids which are not part of the tree");

            if (result.ParentNodeIdsByNodeId.Keys.Any(x => !result.NodesById.ContainsKey(x)))
                throw new Exception("ParentNodeIdByNodeIdMap contains node ids as key that are not part of the syntax tree.");

            if (result.NodesById.Keys.Any(x => !result.ParentNodeIdsByNodeId.ContainsKey(x)))
                throw new Exception("ParentNodeIdByNodeIdMap is missing entries for specific nodes.");

            if (allNodes.Any(x => !ReferenceEquals(result.ParentNodeIdsByNodeId[x.Node.NodeId], x.Parent?.NodeId)))
                throw new Exception("ParentNodeIdByNodeIdMap contains wrong parent ids");
        }
        finally
        {
            Profiler.End("Validating result");
        }
    }
}

public record ParserResult(
    Reader Reader,
    DocumentRootSyntaxNode RootNode,
    ImmutableDictionary<INodeId<SyntaxTreeNode>, SyntaxTreeNode> NodesById,
    ImmutableDictionary<INodeId<SyntaxTreeNode>, INodeId<SyntaxTreeNode>?> ParentNodeIdsByNodeId,
    ImmutableDictionary<Type, ImmutableHashSet<INodeId<SyntaxTreeNode>>> NodeIdsByType
);