using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Inputs
{
    public record GetSyntaxTree(string DocumentPath) : IQuery<GetSyntaxTreeResult>;
    public record GetSyntaxTreeResult(ImmutableDocumentRootSyntaxNode RootNode);
}
