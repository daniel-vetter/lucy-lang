using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetAllEntryPoints() : IQuery<GetAllEntryPointsFunctionsResult>;
    public record GetAllEntryPointsFunctionsResult(ComparableReadOnlyList<FunctionInfo> EntryPoints);

    public class GetAllMainFunctionsHandler : QueryHandler<GetAllEntryPoints, GetAllEntryPointsFunctionsResult>
    {
        public override GetAllEntryPointsFunctionsResult Handle(IDb db, GetAllEntryPoints query)
        {
            var paths = db.Query(new GetDocumentList()).Paths;
            var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
            foreach (var path in paths)
            {
                var ids = db.Query(new GetEntryPointsInDocument(path)).EntryPoints;
                result.AddRange(ids);
            }
            return new GetAllEntryPointsFunctionsResult(result.Build());
        }
    }
}
