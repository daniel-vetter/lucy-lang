using Lucy.Core.Model;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetNodesByType(string DocumentPath, Type Type) : IQuery<GetNodesByTypeResult>;
    public record GetNodesByTypeResult(ComparableReadOnlyList<NodeId> Nodes);

    public class GetNodesByTypesHandler : QueryHandler<GetNodesByType, GetNodesByTypeResult>
    {
        public override GetNodesByTypeResult Handle(IDb db, GetNodesByType query)
        {
            var nodesByType = db.Query(new GetNodeMap(query.DocumentPath)).NodesByType;
            if (nodesByType.TryGetValue(query.Type, out var nodes))       
                return new GetNodesByTypeResult(nodes.Select(x => x.NodeId).ToComparableReadOnlyList());
            return new GetNodesByTypeResult(new ComparableReadOnlyList<NodeId>());
        }
    }

    public record GetFlatNodesByType(string DocumentPath, Type Type) : IQuery<GetFlatNodesByTypeResult>;
    public record GetFlatNodesByTypeResult(ComparableReadOnlyList<FlatSyntaxTreeNode> Nodes);

    public class GetFlatNodesByTypesHandler : QueryHandler<GetFlatNodesByType, GetFlatNodesByTypeResult>
    {
        public override GetFlatNodesByTypeResult Handle(IDb db, GetFlatNodesByType query)
        {
            var result = new ComparableReadOnlyList<FlatSyntaxTreeNode>.Builder();
            var rootNode = new NodeId(query.DocumentPath, "documentRoot[0]").GetFrom(db);
            if (rootNode.GetType() == query.Type)
                result.Add(rootNode);

            result.AddRange(db.Query(new GetChildFlatNodesByType(rootNode.NodeId, query.Type)).Nodes);
            return new GetFlatNodesByTypeResult(result.Build());
        }
    }

    public record GetChildFlatNodesByType(NodeId ParentNodeId, Type Type) : IQuery<GetChildFlatNodesByTypeResult>;
    public record GetChildFlatNodesByTypeResult(ComparableReadOnlyList<FlatSyntaxTreeNode> Nodes);

    public class GetChildFlatNodesByTypesHandler : QueryHandler<GetChildFlatNodesByType, GetChildFlatNodesByTypeResult>
    {
        public override GetChildFlatNodesByTypeResult Handle(IDb db, GetChildFlatNodesByType query)
        {
            var result = new ComparableReadOnlyList<FlatSyntaxTreeNode>.Builder();
            var node = query.ParentNodeId.GetFrom(db);
            foreach (var childNodeId in node.GetChildNodeIds())
            {
                var childNode = childNodeId.GetFrom(db);
                if (childNode.GetType() == query.Type)
                    result.Add(childNode);

                var subChildNodes = db.Query(new GetChildFlatNodesByType(childNode.NodeId, query.Type));
                result.AddRange(subChildNodes.Nodes);
            }
            
            return new GetChildFlatNodesByTypeResult(result.Build());
        }
    }
}
