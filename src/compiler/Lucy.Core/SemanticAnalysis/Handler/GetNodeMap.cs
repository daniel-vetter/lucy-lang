using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetNodeMapResult(ComparableReadOnlyDictionary<NodeId, SyntaxTreeNode> NodesById,
        ComparableReadOnlyDictionary<Type, ComparableReadOnlyList<SyntaxTreeNode>> NodesByType,
        ComparableReadOnlyDictionary<NodeId, NodeId> ParentNodes);
    
    public static class GetNodeMapHandler
    {
        [GenerateDbExtension] ///<see cref="GetNodeMapEx.GetNodeMap"/>
        public static GetNodeMapResult GetNodeMap(IDb db, string documentPath)
        {
            var rootNode = db.GetSyntaxTree(documentPath);
            var nodesByIdBuilder = new ComparableReadOnlyDictionary<NodeId, SyntaxTreeNode>.Builder();
            var parentNodeIds = new ComparableReadOnlyDictionary<NodeId, NodeId>.Builder();

            Traverse(rootNode, nodesByIdBuilder, parentNodeIds);

            var nodesById = nodesByIdBuilder.Build();
            var nodesByType = nodesById.Values
                .GroupBy(x => x.GetType())
                .ToComparableReadOnlyDictionary(x => x.Key, x => x.ToComparableReadOnlyList());

            return new GetNodeMapResult(nodesById, nodesByType, parentNodeIds.Build());
        }

        private static void Traverse(SyntaxTreeNode node, ComparableReadOnlyDictionary<NodeId, SyntaxTreeNode>.Builder nodesById, ComparableReadOnlyDictionary<NodeId, NodeId>.Builder parentNodeIds)
        {
            nodesById.Add(node.NodeId, node);

            foreach (var child in node.GetChildNodes())
            {
                parentNodeIds.Add(child.NodeId, node.NodeId);
                Traverse(child, nodesById, parentNodeIds);
            }
        }
    }
}
