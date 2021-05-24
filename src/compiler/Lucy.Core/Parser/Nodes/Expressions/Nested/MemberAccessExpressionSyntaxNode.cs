using Lucy.Core.Parser.Nodes.Token;
using Lucy.Core.Parser.Nodes.Expressions.Unary;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Expressions.Nested
{
    public class MemberAccessExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public MemberAccessExpressionSyntaxNode(ExpressionSyntaxNode target, TokenNode dotToken, TokenNode memberToken)
        {
            Target = target;
            DotToken = dotToken;
            MemberToken = memberToken;
        }

        public ExpressionSyntaxNode Target { get; }
        public TokenNode DotToken { get; }
        public TokenNode MemberToken { get; }

        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            if (!UnaryExpression.TryRead(code, out result))
                return false;

            while (true)
            {
                if (!TokenNode.TryReadExact(code, ".", out var dotToken))
                    return true;

                if (!TokenNode.TryReadIdentifier(code, out var identifier))
                {
                    code.ReportError("Identifier expected after member access '.'", code.Position);
                    return true;
                }

                result = new MemberAccessExpressionSyntaxNode(result, dotToken, identifier);
            }
        }
    }
}
