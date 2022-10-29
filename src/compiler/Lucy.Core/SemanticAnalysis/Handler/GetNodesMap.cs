using Lucy.Core.Helper;
using Lucy.Core.Parsing;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    /// <summary>
    /// Returns a dictionary of all NodeIds and there corresponding SyntaxTreeNodes of a syntax tree
    /// </summary>
    /// <param name="DocumentPath"></param>
    public record GetNodesMap(string DocumentPath) : IQuery<GetNodesMapResult>;
    public record GetNodesMapResult(ImmutableDictionary<NodeId, SyntaxTreeNode> NodesById, ImmutableDictionary<Type, ImmutableArray<SyntaxTreeNode>> NodesByType);

    public class GetNodesMapHandler : QueryHandler<GetNodesMap, GetNodesMapResult>
    {
        public override GetNodesMapResult Handle(Db db, GetNodesMap query)
        {
            var rootNode = db.Query(new GetSyntaxTree(query.DocumentPath)).RootNode;
            var nodesById = new Dictionary<NodeId, SyntaxTreeNode>();
            
            Traverse(rootNode, nodesById);
            var nodesByType = nodesById.Values
                .GroupBy(x => x.GetType())
                .ToImmutableDictionary(x => x.Key, x => x.ToImmutableArray());
            return new GetNodesMapResult(nodesById.ToImmutableDictionary(), nodesByType);
        }

        private void Traverse(SyntaxTreeNode node, Dictionary<NodeId, SyntaxTreeNode> nodesById)
        {
            nodesById.Add(node.NodeId, node);
            foreach (var child in node.GetChildNodes())
                Traverse(child, nodesById);
        }
    }
}
