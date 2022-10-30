using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using System;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetNodesByType(string DocumentPath, Type Type) : IQuery<GetNodesByTypeResult>;
    public record GetNodesByTypeResult(ComparableReadOnlyList<SyntaxTreeNode> Nodes);

    public class GetNodesByTypesHandler : QueryHandler<GetNodesByType, GetNodesByTypeResult>
    {
        public override GetNodesByTypeResult Handle(Db db, GetNodesByType query)
        {
            var nodesByType = db.Query(new GetNodeMap(query.DocumentPath)).NodesByType;
            if (nodesByType.TryGetValue(query.Type, out var nodes))       
                return new GetNodesByTypeResult(nodes);
            return new GetNodesByTypeResult(new ComparableReadOnlyList<SyntaxTreeNode>());
        }
    }
}
