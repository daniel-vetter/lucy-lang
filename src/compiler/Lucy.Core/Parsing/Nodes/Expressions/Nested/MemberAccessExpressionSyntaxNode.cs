using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Parsing.Nodes.Stuff;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

public static class MemberAccessExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
    {
        result = TryReadOrInner(reader);
        return result != null;
    }

    public static ExpressionSyntaxNode? TryReadOrInner(Reader reader)
    {
        return reader.WithCache(nameof(MemberAccessExpressionSyntaxNodeParser), static (r, _) =>
        {
            if (!UnaryExpression.TryRead(r, out var result))
                return null;

            while (true)
            {
                if (!TokenNodeParser.TryReadExact(r, ".", out var dotToken))
                    return result;

                if (!TokenNodeParser.TryReadIdentifier(r, out var identifier))
                    return MemberAccessExpressionSyntaxNode.Create(result, dotToken, TokenNodeParser.Missing("Identifier expected after member access '.'"));

                result = MemberAccessExpressionSyntaxNode.Create(result, dotToken, identifier);
            }
        });
    }
}