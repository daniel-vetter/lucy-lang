using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public abstract record ScopeEntry;
    public record Scope(ComparableReadOnlyList<ScopeEntry> Entries) : ScopeEntry;
    public record InBetweenNodes(ComparableReadOnlyList<INodeId<SyntaxTreeNode>> NodeIds) : ScopeEntry;
    public record VariableDeclaration : ScopeEntry;
    public record FunctionDeclaration : ScopeEntry;
    
    public static class ScopeHandler
    {

    }
}
