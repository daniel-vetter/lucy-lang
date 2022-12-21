using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

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
        return reader.WithCache(nameof(IfExpressionSyntaxNodeParser), static code =>
        {
            if (!AndExpressionSyntaxNodeParser.TryReadOrInner(code, out var result))
                return null;

            while (true)
            {
                if (!TokenNodeParser.TryReadExact(code, "?", out var ifToken))
                    return result;

                if (!AndExpressionSyntaxNodeParser.TryReadOrInner(code, out var thenExpression))
                    thenExpression = ExpressionSyntaxNodeParser.Missing("Expected expression after '?'");

                if (!TokenNodeParser.TryReadExact(code, ":", out var elseToken))
                    elseToken = TokenNodeParser.Missing("Expected ':'");

                if (!AndExpressionSyntaxNodeParser.TryReadOrInner(code, out var elseExpression))
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