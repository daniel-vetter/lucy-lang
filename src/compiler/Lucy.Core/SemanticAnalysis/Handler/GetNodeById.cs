using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrasturcture;

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

    public record GetFlatNodeById(NodeId NodeId) : IQuery<GetFlatNodeByIdResult>;
    public record GetFlatNodeByIdResult(FlatSyntaxTreeNode Node);

    public class GetFlatNodeByIdHandler : QueryHandler<GetFlatNodeById, GetFlatNodeByIdResult>
    {
        public override GetFlatNodeByIdResult Handle(IDb db, GetFlatNodeById query)
        {
            return new GetFlatNodeByIdResult(db.Query(new GetNodeById(query.NodeId)).Node.ToFlat());
        }
    }
}
