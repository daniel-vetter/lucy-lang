using Lucy.Core.Helper;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    /// <summary>
    /// Returns a dictionary of all NodeIds and there corresponding SyntaxTreeNodes of a syntax tree
    /// </summary>
    /// <param name="DocumentPath"></param>
    public record GetNodeMap(string DocumentPath) : IQuery<GetNodeMapResult>;
    public record GetNodeMapResult(ComparableReadOnlyDictionary<NodeId, SyntaxTreeNode> NodesById, ComparableReadOnlyDictionary<Type, ComparableReadOnlyList<SyntaxTreeNode>> NodesByType);

    public class GetNodeMapHandler : QueryHandler<GetNodeMap, GetNodeMapResult>
    {
        public override GetNodeMapResult Handle(IDb db, GetNodeMap query)
        {
            var rootNode = db.Query(new GetSyntaxTree(query.DocumentPath)).RootNode;
            var nodesById = new Dictionary<NodeId, SyntaxTreeNode>();
            
            Traverse(rootNode, nodesById);
            var nodesByType = nodesById.Values
                .GroupBy(x => x.GetType())
                .ToDictionary(x => x.Key, x => new ComparableReadOnlyList<SyntaxTreeNode>(x));
            return new GetNodeMapResult(new ComparableReadOnlyDictionary<NodeId, SyntaxTreeNode>(nodesById), new ComparableReadOnlyDictionary<Type, ComparableReadOnlyList<SyntaxTreeNode>>(nodesByType));
        }

        private void Traverse(SyntaxTreeNode node, Dictionary<NodeId, SyntaxTreeNode> nodesById)
        {
            nodesById.Add(node.NodeId, node);
            foreach (var child in node.GetChildNodes())
                Traverse(child, nodesById);
        }
    }
}
