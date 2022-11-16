using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetNodesByType(string DocumentPath, Type Type) : IQuery<GetNodesByTypeResult>;
    public record GetNodesByTypeResult(ComparableReadOnlyList<ImmutableSyntaxTreeNode> Nodes);

    public class GetNodesByTypesHandler : QueryHandler<GetNodesByType, GetNodesByTypeResult>
    {
        public override GetNodesByTypeResult Handle(IDb db, GetNodesByType query)
        {
            var nodesByType = db.Query(new GetNodeMap(query.DocumentPath)).NodesByType;
            if (nodesByType.TryGetValue(query.Type, out var nodes))       
                return new GetNodesByTypeResult(nodes.ToComparableReadOnlyList());
            return new GetNodesByTypeResult(new ComparableReadOnlyList<ImmutableSyntaxTreeNode>());
        }
    }
}
