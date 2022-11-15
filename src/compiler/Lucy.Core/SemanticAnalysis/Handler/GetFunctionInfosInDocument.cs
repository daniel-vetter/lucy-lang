using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrasturcture;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetFunctionInfosInDocument(string DocumentPath) : IQuery<GetFunctionInfosInDocumentResult>;
    public record GetFunctionInfosInDocumentResult(ComparableReadOnlyList<FunctionInfo> FunctionInfos);

    public class GetFunctionsInfosInDocumentHandler : QueryHandler<GetFunctionInfosInDocument, GetFunctionInfosInDocumentResult>
    {
        public override GetFunctionInfosInDocumentResult Handle(IDb db, GetFunctionInfosInDocument query)
        {
            var fdList = db.Query(new GetFunctionDeclarations(query.DocumentPath)).Declarations;
            var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
            foreach (var fd in fdList)
                result.Add(db.Query(new GetFunctionInfo(fd)).Info);
            return new GetFunctionInfosInDocumentResult(result.Build());
        }
    }
}
