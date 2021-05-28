using Lucy.Core.Helper;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration;

namespace Lucy.Core.SemanticAnalysis
{
    internal class EntryPointFinder
    {
        internal static void Run(SyntaxNode node)
        {
            if (node is FunctionDeclarationStatementSyntaxNode function)
            {
                var info = function.GetFunctionInfo();
                if (info.Name == "main")
                    info.IsEntryPoint = true;
            }
            
            foreach (var child in node.GetChildNodes())
                Run(child);
        }
    }
}
