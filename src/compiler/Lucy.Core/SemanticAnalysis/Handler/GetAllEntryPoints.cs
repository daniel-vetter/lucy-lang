using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public class GetAllEntryPointsHandler
    {
        [GenerateDbExtension] ///<see cref="GetAllEntryPointsEx.GetAllEntryPoints"/>
        public static ComparableReadOnlyList<FunctionInfo> GetAllEntryPoints(IDb db)
        {
            var paths = db.Query(new GetDocumentList()).Paths;
            var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
            foreach (var path in paths)
            {
                var ids = db.Query(new GetEntryPointsInDocument(path)).EntryPoints;
                result.AddRange(ids);
            }
            return result.Build();
        }
    }
}
