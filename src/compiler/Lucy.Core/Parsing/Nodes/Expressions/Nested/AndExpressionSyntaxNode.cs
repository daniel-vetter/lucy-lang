using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested
{
    internal record AndExpressionSyntaxNode(ExpressionSyntaxNode Left, SyntaxElement AndKeyword, ExpressionSyntaxNode Right) : ExpressionSyntaxNode
    {
        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            if (!OrExpressionSyntaxNode.TryReadOrInner(code, out result))
                return false;

            while (true)
            {
                if (!SyntaxElement.TryReadKeyword(code, "and", out var andToken))
                    return true;

                if (!OrExpressionSyntaxNode.TryReadOrInner(code, out var right))
                {
                    code.ReportError("Expected expression", code.Position);
                    return true;
                }

                result = new AndExpressionSyntaxNode(result, andToken, right);
            }
        }
    }
}
