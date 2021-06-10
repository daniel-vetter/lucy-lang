using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Parsing;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested
{
    public class MemberAccessExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public MemberAccessExpressionSyntaxNode(ExpressionSyntaxNode target, SyntaxElement dotToken, SyntaxElement memberToken)
        {
            Target = target;
            DotToken = dotToken;
            MemberToken = memberToken;
        }

        public ExpressionSyntaxNode Target { get; }
        public SyntaxElement DotToken { get; }
        public SyntaxElement MemberToken { get; }

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
