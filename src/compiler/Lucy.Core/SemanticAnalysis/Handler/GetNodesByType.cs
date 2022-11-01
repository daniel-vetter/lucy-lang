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
}
