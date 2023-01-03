using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Inputs;

public record GetSyntaxTree(string DocumentPath);

public static class GetSyntaxTreeEx
{
    public static DocumentRootSyntaxNode GetSyntaxTree(this IDb db, string documentPath)
    {
        return (DocumentRootSyntaxNode)db.Query(new GetSyntaxTree(documentPath));
    }
}