using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public static class GetNodeByIdHandler
    {
        [GenerateDbExtension] ///<see cref="GetNodeByIdEx.GetNodeById"/>
        public static SyntaxTreeNode GetNodeById(IDb db, NodeId nodeId)
        {
            var nodes = db.GetNodeMap(nodeId.DocumentPath).NodesById;
            return nodes[nodeId];
        }
    }
}
