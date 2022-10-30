using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested
{
    public record AdditionExpressionSyntaxNode(ExpressionSyntaxNode Left, SyntaxElement PlusToken, ExpressionSyntaxNode Right) : ExpressionSyntaxNode
    {
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
