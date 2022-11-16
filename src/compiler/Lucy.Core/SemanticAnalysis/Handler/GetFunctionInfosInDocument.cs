using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public static class GetFunctionsInfosInDocumentHandler
    {
        [GenerateDbExtension] ///<see cref="GetFunctionInfosInDocumentEx.GetFunctionInfosInDocument"/>
        public static ComparableReadOnlyList<FunctionInfo> GetFunctionInfosInDocument(IDb db, string documentPath)
        {
            var fdList = db.GetFunctionDeclarations(documentPath);
            var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
            foreach (var fd in fdList)
                result.Add(db.GetFunctionInfo(fd));
            return result.Build();
        }
    }
}
