using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public static class GetEntryPointsHandler
    {
        [GenerateDbExtension] ///<see cref="GetEntryPointsEx.GetEntryPoints"/>
        public static ComparableReadOnlyList<FunctionInfo> GetEntryPoints(IDb db)
        {
            var paths = db.GetDocumentList();
            var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
            foreach (var path in paths)
            {
                var ids = db.GetEntryPointsInDocument(path);
                result.AddRange(ids);
            }
            return result.Build();
        }

        [GenerateDbExtension] ///<see cref="GetEntryPointsInDocumentEx.GetEntryPointsInDocument"/>
        public static ComparableReadOnlyList<FunctionInfo> GetEntryPointsInDocument(IDb db, string documentPath)
        {
            var infos = db.GetFunctionInfosInDocument(documentPath);
            var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
            foreach(var info in infos)
            {
                if (info.Name == "main")
                    result.Add(info);
            }
            return result.Build();
        }
    }
}
