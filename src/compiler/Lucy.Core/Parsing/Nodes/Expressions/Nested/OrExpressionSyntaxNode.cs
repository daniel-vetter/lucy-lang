using Lucy.Core.Model;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Parsing.Nodes.Stuff;

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
        return reader.WithCache(nameof(OrExpressionSyntaxNodeParser), static (r, _) =>
        {
            if (!AdditionExpressionSyntaxNodeParser.TryReadOrInner(r, out var result))
                return null;

            while (true)
            {
                if (!TokenNodeParser.TryReadKeyword(r, "or", out var orToken))
                    return result;

                if (!AdditionExpressionSyntaxNodeParser.TryReadOrInner(r, out var right))
                    return OrExpressionSyntaxNode.Create(result, orToken, ExpressionSyntaxNodeParser.Missing("Expression expected"));

                result = OrExpressionSyntaxNode.Create(result, orToken, right);
            }
        });
    }
}