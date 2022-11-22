using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors
{
    public static class GetErrors
    {
        [GenerateDbExtension] ///<see cref="GetAllErrorsEx.GetAllErrors"/>
        public static ComparableReadOnlyList<Error> GetAllErrors(IDb db)
        {
            var result = new ComparableReadOnlyList<Error>.Builder();
            result.AddRange(db.GetImportErrors());
            result.AddRange(db.GetEntryPointErrors());

            foreach(var document in db.GetDocumentList())
            {
                
                result.AddRange(db.GetSyntaxErrorsInDocument(document));
                result.AddRange(db.GetDublicateVariableDeclarations(document));
            }
            return result.Build();
        }
    }
}
