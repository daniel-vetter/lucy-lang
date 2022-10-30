using Lucy.Core.Parsing.Nodes;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetFunctionDeclarations(string DocumentPath) : IQuery<GetFunctionDeclarationsResult>;
    public record GetFunctionDeclarationsResult(ComparableReadOnlyList<FunctionDeclarationStatementSyntaxNode> FunctionDeclarations);

    public class GetFunctionDeclarationsHandler : QueryHandler<GetFunctionDeclarations, GetFunctionDeclarationsResult>
    {
        public override GetFunctionDeclarationsResult Handle(Db db, GetFunctionDeclarations query)
        {
            var nodes = db.Query(new GetNodesByType(query.DocumentPath, typeof(FunctionDeclarationStatementSyntaxNode))).Nodes;
            return new GetFunctionDeclarationsResult(new ComparableReadOnlyList<FunctionDeclarationStatementSyntaxNode> (nodes.Cast<FunctionDeclarationStatementSyntaxNode>()));
        }
    }
}
