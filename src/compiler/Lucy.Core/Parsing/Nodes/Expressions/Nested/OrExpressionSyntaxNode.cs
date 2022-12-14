using Lucy.Core.Model;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

internal static class OrExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result)
    {
        result = TryReadOrInner(reader);
        return result != null;
    }

    public static ExpressionSyntaxNodeBuilder? TryReadOrInner(Reader reader)
    {
        return reader.WithCache(nameof(OrExpressionSyntaxNodeParser), static code =>
        {
            if (!AdditionExpressionSyntaxNodeParser.TryReadOrInner(code, out var result))
                return null;

            while (true)
            {
                if (!TokenNodeParser.TryReadKeyword(code, "or", out var orToken))
                    return result;

                if (!AdditionExpressionSyntaxNodeParser.TryReadOrInner(code, out var right))
                    return new OrExpressionSyntaxNodeBuilder(result, orToken, ExpressionSyntaxNodeParser.Missing("Expression expected"));

                result = new OrExpressionSyntaxNodeBuilder(result, orToken, right);
            }
        });
    }
}