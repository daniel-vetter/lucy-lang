using Lucy.Core.Parser.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Expressions.Unary
{
    public class VariableReferenceExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public VariableReferenceExpressionSyntaxNode(SyntaxElement token)
        {
            Token = token;
        }

        public SyntaxElement Token { get; }

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
