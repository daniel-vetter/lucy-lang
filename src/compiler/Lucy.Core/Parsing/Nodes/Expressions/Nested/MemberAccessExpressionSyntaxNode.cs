using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested
{
    public record MemberAccessExpressionSyntaxNode(ExpressionSyntaxNode Target, SyntaxElement DotToken, SyntaxElement MemberToken) : ExpressionSyntaxNode
    {
        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            if (!UnaryExpression.TryRead(code, out result))
                return false;

            while (true)
            {
                if (!SyntaxElement.TryReadExact(code, ".", out var dotToken))
                    return true;

                if (!SyntaxElement.TryReadIdentifier(code, out var identifier))
                {
                    code.ReportError("Identifier expected after member access '.'", code.Position);
                    return true;
                }

                result = new MemberAccessExpressionSyntaxNode(result, dotToken, identifier);
            }
        }
    }
}
