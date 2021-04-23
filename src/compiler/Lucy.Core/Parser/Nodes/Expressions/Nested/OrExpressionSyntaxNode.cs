using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Expressions.Nested
{
    internal class OrExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public OrExpressionSyntaxNode(ExpressionSyntaxNode left, TokenNode token, ExpressionSyntaxNode right)
        {
            Left = left;
            Token = token;
            Right = right;
        }

        public ExpressionSyntaxNode Left { get; }
        public TokenNode Token { get; }
        public ExpressionSyntaxNode Right { get; }

        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            result = null;

            if (!AdditionExpressionSyntaxNode.TryReadOrInner(code, out var left))
                return false;
            
            while (true)
            {
                if (!TokenNode.TryReadKeyword(code, "or", out var orToken))
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
