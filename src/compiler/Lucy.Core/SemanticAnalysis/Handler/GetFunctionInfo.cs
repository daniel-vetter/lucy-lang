using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrasturcture;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetFunctionInfo(NodeId NodeId) : IQuery<GetFunctionInfoResult>;
    public record GetFunctionInfoResult(FunctionInfo Info);

    public record FunctionInfo(NodeId Declaration, string Name, ComparableReadOnlyList<FunctionParameterInfo> Parameters);
    public record FunctionParameterInfo(NodeId Declaration, string Name);

    public class GetFunctionInfoHandler : QueryHandler<GetFunctionInfo, GetFunctionInfoResult>
    {
        public override GetFunctionInfoResult Handle(IDb db, GetFunctionInfo query)
        {
            var node = (ImmutableFunctionDeclarationStatementSyntaxNode)db.Query(new GetNodeById(query.NodeId)).Node;
            var name = node.FunctionName.Token.Text;
            
            var parameters = new ComparableReadOnlyList<FunctionParameterInfo>.Builder();
            foreach(var param in node.ParameterList)
            {
                var paramName = param.VariableDeclaration.VariableName.Token.Text;
                parameters.Add(new FunctionParameterInfo(param.NodeId, paramName));
            }

            var info = new FunctionInfo(
                Declaration: node.NodeId,
                Name: name,
                Parameters: parameters.Build()
            );

            return new GetFunctionInfoResult(info);
        }
    }

}
