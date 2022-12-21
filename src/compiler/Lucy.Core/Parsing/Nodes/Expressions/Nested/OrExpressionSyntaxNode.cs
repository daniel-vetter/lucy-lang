using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

internal static class OrExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
    {
        result = TryReadOrInner(reader);
        return result != null;
    }

    public static ExpressionSyntaxNode? TryReadOrInner(Reader reader)
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
                    return OrExpressionSyntaxNode.Create(result, orToken, ExpressionSyntaxNodeParser.Missing("Expression expected"));

                result = OrExpressionSyntaxNode.Create(result, orToken, right);
            }
        });
    }
}