using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes.Expressions;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested
{
    public class AdditionExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public AdditionExpressionSyntaxNode(ExpressionSyntaxNode left, SyntaxElement plusToken, ExpressionSyntaxNode right)
        {
            Left = left;
            PlusToken = plusToken;
            Right = right;
        }

        public ExpressionSyntaxNode Left { get; }
        public SyntaxElement PlusToken { get; }
        public ExpressionSyntaxNode Right { get; }

        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            if (!MemberAccessExpressionSyntaxNode.TryReadOrInner(code, out result))
                return false;

            while (true)
            {
                if (!SyntaxElement.TryReadExact(code, "+", out var plusToken))
                    return true;

                if (!MemberAccessExpressionSyntaxNode.TryReadOrInner(code, out var right))
                    return true;

                result = new AdditionExpressionSyntaxNode(result, plusToken, right);
            }
        }
    }
}
