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
            var nodeIds = db.Query(new GetFunctionDeclarations(query.DocumentPath)).Ids;
            var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
            foreach (var nodeId in nodeIds)
                result.Add(db.Query(new GetFunctionInfo(nodeId)).Info);
            return new GetFunctionInfosInDocumentResult(result.Build());
        }
    }
}
