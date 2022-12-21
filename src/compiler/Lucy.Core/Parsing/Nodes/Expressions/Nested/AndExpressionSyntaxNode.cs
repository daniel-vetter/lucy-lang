using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

internal static class AndExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
    {
        result = TryReadOrInner(reader);
        return result != null;
    }

    public static ExpressionSyntaxNode? TryReadOrInner(Reader reader)
    {
        return reader.WithCache(nameof(AndExpressionSyntaxNodeParser), static r =>
        {
            if (!OrExpressionSyntaxNodeParser.TryReadOrInner(r, out var result))
                return null;

            while (true)
            {
                if (!TokenNodeParser.TryReadKeyword(r, "and", out var andToken))
                    return result;

                if (!OrExpressionSyntaxNodeParser.TryReadOrInner(r, out var right))
                    return AndExpressionSyntaxNode.Create(result, andToken, ExpressionSyntaxNodeParser.Missing("Expression expected"));

                result = AndExpressionSyntaxNode.Create(result, andToken, right);
            }
        });
    }
}