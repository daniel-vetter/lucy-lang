using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Inputs;

public record GetDocumentList;

public static class GetDocumentListEx
{
    public static ComparableReadOnlyList<string> GetDocumentList(this IDb db) => (ComparableReadOnlyList<string>)db.Query(new GetDocumentList());
}