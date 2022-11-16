using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetNodesByType(string DocumentPath, Type Type) : IQuery<GetNodesByTypeResult>;
    public record GetNodesByTypeResult(ComparableReadOnlyList<SyntaxTreeNode> Nodes);

    public class GetNodesByTypesHandler : QueryHandler<GetNodesByType, GetNodesByTypeResult>
    {
        public override GetNodesByTypeResult Handle(IDb db, GetNodesByType query)
        {
            var nodesByType = db.GetNodeMap(query.DocumentPath).NodesByType;
            if (nodesByType.TryGetValue(query.Type, out var nodes))       
                return new GetNodesByTypeResult(nodes.ToComparableReadOnlyList());
            return new GetNodesByTypeResult(new ComparableReadOnlyList<SyntaxTreeNode>());
        }
    }
}
