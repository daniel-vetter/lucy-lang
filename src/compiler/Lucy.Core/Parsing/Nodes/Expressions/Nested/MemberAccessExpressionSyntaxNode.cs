using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested
{
    public class MemberAccessExpressionSyntaxNodeParser
    {
        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result)
        {
            if (!UnaryExpression.TryRead(code, out result))
                return false;

            while (true)
            {
                if (!SyntaxElementParser.TryReadExact(code, ".", out var dotToken))
                    return true;

                if (!SyntaxElementParser.TryReadIdentifier(code, out var identifier))
                {
                    result = new MemberAccessExpressionSyntaxNodeBuilder(result, dotToken, SyntaxElementParser.Missing("Identifier expected after member access '.'"));
                    return true;
                }

                result = new MemberAccessExpressionSyntaxNodeBuilder(result, dotToken, identifier);
            }
        }
    }
}
