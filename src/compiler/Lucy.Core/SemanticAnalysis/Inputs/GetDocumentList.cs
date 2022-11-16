using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Inputs;

public record GetDocumentList() : IQuery<GetDocumentListResult>;
public record GetDocumentListResult(ComparableReadOnlyList<string> Paths);