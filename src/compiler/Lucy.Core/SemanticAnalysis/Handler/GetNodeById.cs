using Lucy.Core.Parsing;
using Lucy.Core.SemanticAnalysis.Infrasturcture;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetNodeById(NodeId NodeId) : IQuery<GetNodeByIdResult>;
    public record GetNodeByIdResult(SyntaxTreeNode Node);

    public class GetNodeByIdHandler : QueryHandler<GetNodeById, GetNodeByIdResult>
    {

        private static int counter = 0;
        public override GetNodeByIdResult Handle(Db db, GetNodeById query)
        {
            var nodes = db.Query(new GetNodeMap(query.NodeId.DocumentPath)).NodesById;
            return new GetNodeByIdResult(nodes[query.NodeId]);
        }
    }
}
