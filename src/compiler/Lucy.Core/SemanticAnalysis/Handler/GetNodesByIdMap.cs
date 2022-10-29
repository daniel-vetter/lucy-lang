using Lucy.Core.Helper;
using Lucy.Core.Parsing;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    /// <summary>
    /// Returns a dictionary of all NodeIds and there corresponding SyntaxTreeNodes of a syntax tree
    /// </summary>
    /// <param name="DocumentPath"></param>
    public record GetNodesMap(string DocumentPath) : IQuery<GetNodesMapResult>;
    public record GetNodesMapResult(ImmutableDictionary<NodeId, SyntaxTreeNode> NodesById);

    public class GetNodesByIdMapHandler : QueryHandler<GetNodesMap, GetNodesMapResult>
    {
        public override GetNodesMapResult Handle(Db db, GetNodesMap query)
        {
            var rootNode = db.Query(new GetSyntaxTree(query.DocumentPath)).RootNode;
            var dictionary = new Dictionary<NodeId, SyntaxTreeNode>();
            Traverse(rootNode, dictionary);
            return new GetNodesMapResult(dictionary.ToImmutableDictionary());
        }

        private void Traverse(SyntaxTreeNode node, Dictionary<NodeId, SyntaxTreeNode> nodesById)
        {
            nodesById.Add(node.NodeId, node);
            foreach (var child in node.GetChildNodes())
                Traverse(child, nodesById);
        }
    }
}
