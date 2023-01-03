using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Lucy.Common;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing;

public static class ParserResultValidator
{
    [Conditional("CUSTOM_DEBUG")]
    public static void Validate(string code, ParserResult result)
    {
        Profiler.Start("Validating result");
        try
        {
            List<(SyntaxTreeNode? Parent, SyntaxTreeNode Node)> allNodes = new();
            var sb = new StringBuilder();

            void Traverse(SyntaxTreeNode? parent, SyntaxTreeNode node)
            {
                allNodes.Add((parent, node));
                if (node is TokenNode t)
                {
                    sb.Append(t.Text);
                    sb.Append(t.TrailingTrivia);
                }
                foreach (var child in node.GetChildNodes())
                    Traverse(node, child);
            }
            Traverse(null, result.RootNode);

            var codeFromNodes = sb.ToString();
            if (codeFromNodes != code)
                throw new Exception("SyntaxTree did not match the parsed code");

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

            var allNodesByType = allNodes.GroupBy(x => x.Node.GetType()).ToDictionary(x => x.Key, x => x.Select(y => y.Node.NodeId).ToHashSet());
            var tooManyTypes = result.NodeIdsByType.Keys.Where(x => !allNodesByType.ContainsKey(x));
            if (tooManyTypes.Any())
                throw new Exception("NodeIdsByType map contains to many types");

            if (allNodesByType.Keys.Any(x => !result.NodeIdsByType.ContainsKey(x)))
                throw new Exception("NodeIdsByType map contains not all types");

            foreach (var (type, expectedNodes) in allNodesByType)
            {
                var nodes = result.NodeIdsByType[type];

                if (nodes.Any(x => !expectedNodes.Contains(x)))
                    throw new Exception("NodeIdsByType map has to many nodes for type " + type);

                if (expectedNodes.Any(x => !nodes.Contains(x)))
                    throw new Exception("NodeIdsByType map has to no enough for type " + type);
            }
        }
        finally
        {
            Profiler.End("Validating result");
        }
    }
}