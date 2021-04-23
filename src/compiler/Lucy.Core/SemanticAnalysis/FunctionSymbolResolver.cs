using Lucy.Core.Helper;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Expressions.Unary;

namespace Lucy.Core.SemanticAnalysis
{
    internal class FunctionSymbolResolver
    {
        internal static void Run(SyntaxNode node)
        {
            if (node is FunctionCallExpressionSyntaxNode functionCall)
            {
                var matchingFunctions = functionCall.GetScope().GetAllMatchingSymbols(functionCall.FunctionName.Value);
                if (matchingFunctions.Length == 0)
                {
                    functionCall.FunctionName.AddError($"Could not find a function called '{functionCall.FunctionName.Value}' in the current context.");
                }
                else if (matchingFunctions.Length == 1)
                {
                    functionCall.SetAnnotation(matchingFunctions[0]);
                }
                else if (matchingFunctions.Length > 1)
                {
                    functionCall.FunctionName.AddError($"Found more than one function named '{functionCall.FunctionName.Value}' in the current context.");
                }
            }

            foreach (var child in node.GetChildNodes())
                Run(child.Node);
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
