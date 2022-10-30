using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary
{
    public record VariableReferenceExpressionSyntaxNode(SyntaxElement Token) : ExpressionSyntaxNode
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out VariableReferenceExpressionSyntaxNode? result)
        {
            if (SyntaxElement.TryReadIdentifier(code, out var token))
            {
                result = new VariableReferenceExpressionSyntaxNode(token);
                return true;
            }

            result = null;
            return false;
        }
    }
}
