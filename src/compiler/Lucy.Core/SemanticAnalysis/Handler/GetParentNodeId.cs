using Lucy.Core.Model;
using Lucy.Core.Parsing;
using Lucy.Core.SemanticAnalysis.Infrasturcture;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetParentNodeId(NodeId NodeId) : IQuery<GetParentNodeIdResult>;
    public record GetParentNodeIdResult(NodeId? ParentNodeId);

    public class GetParentNodeIdHandler : QueryHandler<GetParentNodeId, GetParentNodeIdResult>
    {
        public override GetParentNodeIdResult Handle(IDb db, GetParentNodeId query)
        {
            var map = db.Query(new GetNodeMap(query.NodeId.DocumentPath));
            if (map.ParentNodes.TryGetValue(query.NodeId, out var result))
                return new GetParentNodeIdResult(result);
            return new GetParentNodeIdResult(null);
        }
    }
}
