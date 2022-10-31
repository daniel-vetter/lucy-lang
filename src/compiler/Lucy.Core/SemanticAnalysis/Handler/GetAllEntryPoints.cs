using Lucy.Core.Parsing.Nodes;
using Lucy.Core.Parsing;
using Lucy.Core.SemanticAnalysis.Infrasturcture;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public record GetAllEntryPoints() : IQuery<GetAllEntryPointsFunctionsResult>;
    public record GetAllEntryPointsFunctionsResult(ComparableReadOnlyList<NodeId> EntryPoints);

    public class GetAllMainFunctionsHandler : QueryHandler<GetAllEntryPoints, GetAllEntryPointsFunctionsResult>
    {
        public override GetAllEntryPointsFunctionsResult Handle(Db db, GetAllEntryPoints query)
        {
            var paths = db.Query(new GetDocumentList()).Paths;
            var result = new ComparableReadOnlyList<NodeId>.Builder();
            foreach (var path in paths)
            {
                var ids = db.Query(new GetEntryPointsInDocument(path)).EntryPoints;
                result.AddRange(ids);
            }
            return new GetAllEntryPointsFunctionsResult(result.Build());
        }
    }
}
