using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Inputs
{
    public record GetSyntaxTree(string DocumentPath) : IQuery<GetSyntaxTreeResult>;
    public record GetSyntaxTreeResult(DocumentRootSyntaxNode RootNode);

    public static class GetSyntaxTreeEx
    {
        public static DocumentRootSyntaxNode GetSyntaxTree(this IDb db, string documentPath)
        {
            return db.Query(new GetSyntaxTree(documentPath)).RootNode;
        }
    }
}
