using Lucy.Core.Model.Syntax;

namespace Lucy.Core.SemanticAnalysis
{
    public class SemanticAnalyzer
    {
        public static void Run(SyntaxNode node)
        {
            ParentAssigner.Run(node);
            ScopeAssigner.Run(node);
            TypeDiscovery.Run(node);
            FunctionSymbolResolver.Run(node);
        }
    }
}
