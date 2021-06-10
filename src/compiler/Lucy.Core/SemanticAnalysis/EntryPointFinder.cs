using Lucy.Core.Helper;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.Parsing;

namespace Lucy.Core.SemanticAnalysis
{
    internal class EntryPointFinder
    {
        internal static void Run(SyntaxTreeNode node)
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
