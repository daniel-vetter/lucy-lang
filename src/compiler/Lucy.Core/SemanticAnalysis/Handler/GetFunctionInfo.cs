using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record FunctionInfo(NodeId Declaration, string Name, ComparableReadOnlyList<FunctionParameterInfo> Parameters);
    public record FunctionParameterInfo(NodeId Declaration, string Name);

    public static class GetFunctionInfoHandler
    {
        [GenerateDbExtension] ///<see cref="GetFunctionInfoEx.GetFunctionInfo"/>
        public static FunctionInfo GetFunctionInfo(IDb db, FunctionDeclarationStatementSyntaxNode declaration)
        {
            var name = declaration.FunctionName.Token.Text;
            
            var parameters = new ComparableReadOnlyList<FunctionParameterInfo>.Builder();
            foreach(var param in declaration.ParameterList)
            {
                var paramName = param.VariableDeclaration.VariableName.Token.Text;
                parameters.Add(new FunctionParameterInfo(param.NodeId, paramName));
            }

            var info = new FunctionInfo(
                Declaration: declaration.NodeId,
                Name: name,
                Parameters: parameters.Build()
            );

            return info;
        }
    }
}
