using Lucy.Core.Model;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

internal static class AndExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result)
    {
        result = TryReadOrInner(reader);
        return result != null;
    }

    public static ExpressionSyntaxNodeBuilder? TryReadOrInner(Reader reader)
    {
        return reader.WithCache(nameof(AndExpressionSyntaxNodeParser), static code =>
        {
            if (!OrExpressionSyntaxNodeParser.TryReadOrInner(code, out var result))
                return null;

            while (true)
            {
                if (!TokenNodeParser.TryReadKeyword(code, "and", out var andToken))
                    return result;

                if (!OrExpressionSyntaxNodeParser.TryReadOrInner(code, out var right))
                    return new AndExpressionSyntaxNodeBuilder(result, andToken, ExpressionSyntaxNodeParser.Missing("Expression expected"));

                result = new AndExpressionSyntaxNodeBuilder(result, andToken, right);
            }
        });
    }
}