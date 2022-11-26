using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Inputs;

public record GetDocumentList : IQuery<GetDocumentListResult>;
public record GetDocumentListResult(ComparableReadOnlyList<string> Paths);

public static class GetDocumentListEx
{
    public static ComparableReadOnlyList<string> GetDocumentList(this IDb db) => db.Query(new GetDocumentList()).Paths;
}