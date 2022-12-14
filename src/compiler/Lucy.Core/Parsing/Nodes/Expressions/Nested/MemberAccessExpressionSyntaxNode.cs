using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

public static class MemberAccessExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result)
    {
        result = TryReadOrInner(reader);
        return result != null;
    }

    public static ExpressionSyntaxNodeBuilder? TryReadOrInner(Reader reader)
    {
        return reader.WithCache(nameof(MemberAccessExpressionSyntaxNodeParser), static code =>
        {
            if (!UnaryExpression.TryRead(code, out var result))
                return null;

            while (true)
            {
                if (!TokenNodeParser.TryReadExact(code, ".", out var dotToken))
                    return result;

                if (!TokenNodeParser.TryReadIdentifier(code, out var identifier))
                    return new MemberAccessExpressionSyntaxNodeBuilder(result, dotToken, TokenNodeParser.Missing("Identifier expected after member access '.'"));

                result = new MemberAccessExpressionSyntaxNodeBuilder(result, dotToken, identifier);
            }
        });
    }
}