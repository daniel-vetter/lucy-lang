using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes.Expressions;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested
{
    internal class OrExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public OrExpressionSyntaxNode(ExpressionSyntaxNode left, SyntaxElement token, ExpressionSyntaxNode right)
        {
            Left = left;
            Token = token;
            Right = right;
        }

        public ExpressionSyntaxNode Left { get; }
        public SyntaxElement Token { get; }
        public ExpressionSyntaxNode Right { get; }

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
