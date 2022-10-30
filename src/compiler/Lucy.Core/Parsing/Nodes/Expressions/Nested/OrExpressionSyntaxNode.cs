using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested
{
    internal record OrExpressionSyntaxNode(ExpressionSyntaxNode Left, SyntaxElement Token, ExpressionSyntaxNode Right) : ExpressionSyntaxNode
    {
        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            result = null;

            if (!AdditionExpressionSyntaxNode.TryReadOrInner(code, out var left))
                return false;
            
            while (true)
            {
                if (!SyntaxElement.TryReadKeyword(code, "or", out var orToken))
                {
                    result = left;
                    return true;
                }
                
                if (!AdditionExpressionSyntaxNode.TryReadOrInner(code, out var right))
                {
                    result = left;
                    code.ReportError("Expression expected", code.Position);
                    return true;
                }

                left = new OrExpressionSyntaxNode(left, orToken, right);
            }
        }
    }
}
