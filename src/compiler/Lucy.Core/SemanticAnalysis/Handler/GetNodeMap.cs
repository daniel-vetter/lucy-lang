using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    /// <summary>
    /// Returns a dictionary of all NodeIds and there corresponding SyntaxTreeNodes of a syntax tree
    /// </summary>
    /// <param name="DocumentPath"></param>
    public record GetNodeMap(string DocumentPath) : IQuery<GetNodeMapResult>;
    public record GetNodeMapResult(ComparableReadOnlyDictionary<NodeId, ImmutableSyntaxTreeNode> NodesById,
        ComparableReadOnlyDictionary<Type, ComparableReadOnlyList<ImmutableSyntaxTreeNode>> NodesByType,
        ComparableReadOnlyDictionary<NodeId, NodeId> ParentNodes);
    
    public class GetNodeMapHandler : QueryHandler<GetNodeMap, GetNodeMapResult>
    {
        public override GetNodeMapResult Handle(IDb db, GetNodeMap query)
        {
            var rootNode = db.Query(new GetSyntaxTree(query.DocumentPath)).RootNode;
            var nodesByIdBuilder = new ComparableReadOnlyDictionary<NodeId, ImmutableSyntaxTreeNode>.Builder();
            var parentNodeIds = new ComparableReadOnlyDictionary<NodeId, NodeId>.Builder();

            Traverse(rootNode, nodesByIdBuilder, parentNodeIds);

            var nodesById = nodesByIdBuilder.Build();
            var nodesByType = nodesById.Values
                .GroupBy(x => x.GetType())
                .ToComparableReadOnlyDictionary(x => x.Key, x => x.ToComparableReadOnlyList());

            return new GetNodeMapResult(nodesById, nodesByType, parentNodeIds.Build());
        }

        private void Traverse(ImmutableSyntaxTreeNode node, ComparableReadOnlyDictionary<NodeId, ImmutableSyntaxTreeNode>.Builder nodesById, ComparableReadOnlyDictionary<NodeId, NodeId>.Builder parentNodeIds)
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
