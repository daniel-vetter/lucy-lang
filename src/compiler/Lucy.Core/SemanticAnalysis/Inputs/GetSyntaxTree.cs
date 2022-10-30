using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrasturcture;

namespace Lucy.Core.SemanticAnalysis.Inputs
{
    public record GetSyntaxTree(string DocumentPath) : IQuery<GetSyntaxTreeResult>;
    public record GetSyntaxTreeResult(DocumentRootSyntaxNode RootNode);
}
