using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using System;
using System.Linq.Expressions;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetFunctionInfo(ImmutableFunctionDeclarationStatementSyntaxNode Declaration) : IQuery<GetFunctionInfoResult>;
    public record GetFunctionInfoResult(FunctionInfo Info);

    public record FunctionInfo(NodeId Declaration, string Name, ComparableReadOnlyList<FunctionParameterInfo> Parameters);
    public record FunctionParameterInfo(NodeId Declaration, string Name);

    
    public class GetFunctionInfoHandler : QueryHandler<GetFunctionInfo, GetFunctionInfoResult>
    {
        public void Run(int test)
        {
            
        }

        public override GetFunctionInfoResult Handle(IDb db, GetFunctionInfo query)
        {
            var name = query.Declaration.FunctionName.Token.Text;
            
            var parameters = new ComparableReadOnlyList<FunctionParameterInfo>.Builder();
            foreach(var param in query.Declaration.ParameterList)
            {
                var paramName = param.VariableDeclaration.VariableName.Token.Text;
                parameters.Add(new FunctionParameterInfo(param.NodeId, paramName));
            }

            var info = new FunctionInfo(
                Declaration: query.Declaration.NodeId,
                Name: name,
                Parameters: parameters.Build()
            );

            var r = db.DoSome("awd");

            return new GetFunctionInfoResult(info);
        }
    }

    public static class MyHandler
    {
        [DbHelper.DbQueryHandlerAttribute()] /// <see cref="DoSomeExtension.DoSome"/>
        public static int DoSome(IDb db, string blub)
        {
            return 0;
        }
    }

    public class TestAttribute : System.Attribute
    {
        public delegate object Del(IDb db);

        public TestAttribute(string exp)
        {

        }
    }
}
