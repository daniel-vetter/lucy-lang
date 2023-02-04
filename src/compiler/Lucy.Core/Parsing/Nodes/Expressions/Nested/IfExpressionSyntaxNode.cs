using Lucy.Core.Model;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Parsing.Nodes.Stuff;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

public static class IfExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
    {
        result = TryReadOrInner(reader);
        return result != null;
    }

    public static ExpressionSyntaxNode? TryReadOrInner(Reader reader)
    {
        return reader.WithCache(nameof(IfExpressionSyntaxNodeParser), static (r, _) =>
        {
            if (!AndExpressionSyntaxNodeParser.TryReadOrInner(r, out var result))
                return null;

            while (true)
            {
                if (!TokenNodeParser.TryReadExact(r, "?", out var ifToken))
                    return result;

                if (!AndExpressionSyntaxNodeParser.TryReadOrInner(r, out var thenExpression))
                    thenExpression = ExpressionSyntaxNodeParser.Missing("Expected expression after '?'");

                if (!TokenNodeParser.TryReadExact(r, ":", out var elseToken))
                    elseToken = TokenNodeParser.Missing("Expected ':'");

                if (!AndExpressionSyntaxNodeParser.TryReadOrInner(r, out var elseExpression))
                    elseExpression = ExpressionSyntaxNodeParser.Missing("Expression expected after ':'");

                result = IfExpressionSyntaxNode.Create(
                    condition: result,
                    ifToken: ifToken,
                    thenExpression: thenExpression,
                    elseToken: elseToken,
                    elseExpression: elseExpression
                );
            }
        });
    }
}