using Lucy.Core.Helper;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Parsing;

namespace Lucy.Core.SemanticAnalysis
{
    internal class FunctionSymbolResolver
    {
        internal static void Run(SyntaxTreeNode node, SemanticModel semanticModel)
        {
            if (node is FunctionCallExpressionSyntaxNode functionCall)
            {
                var matchingFunctions = semanticModel.GetScope(functionCall).GetAllMatchingSymbols(functionCall.FunctionName.Token.Text);
                if (matchingFunctions.Length == 0)
                {
                    semanticModel.AddErrorIssue(functionCall, $"Could not find a function called '{functionCall.FunctionName.Token.Text}' in the current context.");
                }
                else if (matchingFunctions.Length == 1)
                {
                    semanticModel.SetFunctionInfo(functionCall, (FunctionInfo)matchingFunctions[0]);
                }
                else if (matchingFunctions.Length > 1)
                {
                    semanticModel.AddErrorIssue(functionCall, $"Found more than one function named '{functionCall.FunctionName.Token.Text}' in the current context.");
                }
            }

            foreach (var child in node.GetChildNodes())
                Run(child, semanticModel);
        }
    }
}
