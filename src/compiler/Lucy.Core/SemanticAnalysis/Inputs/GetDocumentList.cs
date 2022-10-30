using Lucy.Core.SemanticAnalysis.Infrasturcture;
using Lucy.Core.Parsing.Nodes;

namespace Lucy.Core.SemanticAnalysis.Inputs;

public record GetDocumentList() : IQuery<GetDocumentListResult>;
public record GetDocumentListResult(ComparableReadOnlyList<string> Paths);