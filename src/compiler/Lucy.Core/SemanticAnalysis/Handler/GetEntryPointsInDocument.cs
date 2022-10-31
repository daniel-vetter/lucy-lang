using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.SemanticAnalysis.Infrasturcture;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetEntryPointsInDocument(string DocumentPath) : IQuery<GetEntryPointsInDocumentResult>;
    public record GetEntryPointsInDocumentResult(ComparableReadOnlyList<NodeId> EntryPoints);

    public class GetMainFunctionsInDocumentHandler : QueryHandler<GetEntryPointsInDocument, GetEntryPointsInDocumentResult>
    {
        public override GetEntryPointsInDocumentResult Handle(Db db, GetEntryPointsInDocument query)
        {
            var ids = db.Query(new GetFunctionDeclarations(query.DocumentPath)).Ids;
            var result = new ComparableReadOnlyList<NodeId>.Builder();
            foreach(var id in ids)
            {
                var node = (FunctionDeclarationStatementSyntaxNode)db.Query(new GetNodeById(id)).Node;
                if (node.FunctionName.Token.Text == "main")
                    result.Add(id);
            }
            return new GetEntryPointsInDocumentResult(result.Build());
        }
    }
}
