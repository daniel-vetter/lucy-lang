using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

internal static class AndExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result)
    {
        if (!OrExpressionSyntaxNodeParser.TryReadOrInner(code, out result))
            return false;

        while (true)
        {
            if (!SyntaxElementParser.TryReadKeyword(code, "and", out var andToken))
                return true;

            if (!OrExpressionSyntaxNodeParser.TryReadOrInner(code, out var right))
            {
                right = new MissingExpressionSyntaxNodeBuilder()
                {
                    SyntaxErrors = { { "Expected expression" } }
                };
                result = new AndExpressionSyntaxNodeBuilder(result, andToken, right);
                return true;
            }

            result = new AndExpressionSyntaxNodeBuilder(result, andToken, right);
        }
    }
}