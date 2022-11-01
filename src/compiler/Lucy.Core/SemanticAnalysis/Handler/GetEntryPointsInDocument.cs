using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrasturcture;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetEntryPointsInDocument(string DocumentPath) : IQuery<GetEntryPointsInDocumentResult>;
    public record GetEntryPointsInDocumentResult(ComparableReadOnlyList<FunctionInfo> EntryPoints);

    public class GetMainFunctionsInDocumentHandler : QueryHandler<GetEntryPointsInDocument, GetEntryPointsInDocumentResult>
    {
        public override GetEntryPointsInDocumentResult Handle(IDb db, GetEntryPointsInDocument query)
        {
            var infos = db.Query(new GetFunctionInfosInDocument(query.DocumentPath)).FunctionInfos;
            var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
            foreach(var info in infos)
            {
                if (info.Name == "main")
                    result.Add(info);
            }
            return new GetEntryPointsInDocumentResult(result.Build());
        }
    }
}
