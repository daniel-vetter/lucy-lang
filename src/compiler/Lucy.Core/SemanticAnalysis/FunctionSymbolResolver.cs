using Lucy.Core.Helper;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Parsing;

namespace Lucy.Core.SemanticAnalysis
{
    internal class FunctionSymbolResolver
    {
        internal static void Run(SyntaxTreeNode node)
        {
            if (node is FunctionCallExpressionSyntaxNode functionCall)
            {
                var matchingFunctions = functionCall.GetScope().GetAllMatchingSymbols(functionCall.FunctionName.Token.Text);
                if (matchingFunctions.Length == 0)
                {
                    functionCall.FunctionName.AddError($"Could not find a function called '{functionCall.FunctionName.Token.Text}' in the current context.");
                }
                else if (matchingFunctions.Length == 1)
                {
                    functionCall.SetAnnotation(matchingFunctions[0]);
                }
                else if (matchingFunctions.Length > 1)
                {
                    functionCall.FunctionName.AddError($"Found more than one function named '{functionCall.FunctionName.Token.Text}' in the current context.");
                }
            }

            foreach (var child in node.GetChildNodes())
                Run(child);
        }
    }

    public static class FunctionCallInfoEx
    {
        public static FunctionInfo? GetFunctionInfo(this FunctionCallExpressionSyntaxNode node)
        {
            return node.GetAnnotation<FunctionInfo>();
        }
    }
}
