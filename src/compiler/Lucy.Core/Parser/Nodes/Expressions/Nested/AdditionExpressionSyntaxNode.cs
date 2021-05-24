using Lucy.Core.Parser.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Expressions.Nested
{
    public class AdditionExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public AdditionExpressionSyntaxNode(ExpressionSyntaxNode left, TokenNode plusToken, ExpressionSyntaxNode right)
        {
            Left = left;
            PlusToken = plusToken;
            Right = right;
        }

        public ExpressionSyntaxNode Left { get; }
        public TokenNode PlusToken { get; }
        public ExpressionSyntaxNode Right { get; }

        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            if (!MemberAccessExpressionSyntaxNode.TryReadOrInner(code, out result))
                return false;

            while (true)
            {
                if (!TokenNode.TryReadExact(code, "+", out var plusToken))
                    return true;

                if (!MemberAccessExpressionSyntaxNode.TryReadOrInner(code, out var right))
                    return true;

                result = new AdditionExpressionSyntaxNode(result, plusToken, right);
            }
        }
    }
}
