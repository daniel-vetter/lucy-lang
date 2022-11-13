using Lucy.Core.Model;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetFunctionDeclarations(string DocumentPath) : IQuery<GetFunctionDeclarationsResult>;
    public record GetFunctionDeclarationsResult(ComparableReadOnlyList<FlatFunctionDeclarationStatementSyntaxNode> Ids);

    public class GetFunctionDeclarationsHandler : QueryHandler<GetFunctionDeclarations, GetFunctionDeclarationsResult>
    {
        public override GetFunctionDeclarationsResult Handle(IDb db, GetFunctionDeclarations query)
        {
            var r = db.Query(new GetFlatNodesByType(query.DocumentPath, typeof(FlatFunctionDeclarationStatementSyntaxNode)));
            return new GetFunctionDeclarationsResult(r.Nodes.Cast<FlatFunctionDeclarationStatementSyntaxNode>().ToComparableReadOnlyList());
        }
    }
}
