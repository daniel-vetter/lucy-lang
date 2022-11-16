using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public static class GetMainFunctionsInDocumentHandler
    {
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
