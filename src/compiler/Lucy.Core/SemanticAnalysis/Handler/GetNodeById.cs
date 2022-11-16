using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetNodeById(NodeId NodeId) : IQuery<GetNodeByIdResult>;
    public record GetNodeByIdResult(ImmutableSyntaxTreeNode Node);

    public class GetNodeByIdHandler : QueryHandler<GetNodeById, GetNodeByIdResult>
    {
        public override GetNodeByIdResult Handle(IDb db, GetNodeById query)
        {
            var nodes = db.Query(new GetNodeMap(query.NodeId.DocumentPath)).NodesById;
            return new GetNodeByIdResult(nodes[query.NodeId]);
        }
    }
}
