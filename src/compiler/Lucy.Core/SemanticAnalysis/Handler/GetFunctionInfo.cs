using Lucy.Core.Model;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.SemanticAnalysis.Infrasturcture;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetFunctionInfo(NodeId<FlatFunctionDeclarationStatementSyntaxNode> NodeId) : IQuery<GetFunctionInfoResult>;
    public record GetFunctionInfoResult(FunctionInfo Info);

    public record FunctionInfo(NodeId Declaration, string Name, ComparableReadOnlyList<FunctionParameterInfo> Parameters);
    public record FunctionParameterInfo(NodeId Declaration, string Name);

    public class GetFunctionInfoHandler : QueryHandler<GetFunctionInfo, GetFunctionInfoResult>
    {
        public override GetFunctionInfoResult Handle(IDb db, GetFunctionInfo query)
        {
            var node = query.NodeId.Get(db);
            var name = node.FunctionName.Get(db).Token.Get(db).Text;
            
            var parameters = new ComparableReadOnlyList<FunctionParameterInfo>.Builder();
            foreach(var param in node.ParameterList)
            {
                var paramName = param.Get(db).VariableDeclaration.Get(db).VariableName.Get(db).Token.Get(db).Text;
                parameters.Add(new FunctionParameterInfo(param.Get(db).NodeId, paramName));
            }

            var info = new FunctionInfo(
                Declaration: node.NodeId,
                Name: name,
                Parameters: parameters.Build()
            );

            return new GetFunctionInfoResult(info);
        }
    }

    public static class DbEx
    {
        public static FlatSyntaxTreeNode GetFrom(this NodeId nodeId, IDb db)
        {
            return (db.Query(new GetFlatNodeById(new NodeId(nodeId.DocumentPath, nodeId.NodePath))).Node);
        }

        public static T Get<T>(this NodeId<T> nodeId, IDb db) where T : FlatSyntaxTreeNode
        {
            return (T)(db.Query(new GetFlatNodeById(new NodeId(nodeId.DocumentPath, nodeId.NodePath))).Node);
        }
    }

}
