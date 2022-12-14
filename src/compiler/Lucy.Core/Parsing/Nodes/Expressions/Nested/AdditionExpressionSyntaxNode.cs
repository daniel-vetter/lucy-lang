using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

public static class AdditionExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result)
    {
        result = TryReadOrInner(reader);
        return result != null;
    }

    public static ExpressionSyntaxNodeBuilder? TryReadOrInner(Reader reader)
    {
        return reader.WithCache(nameof(AdditionExpressionSyntaxNodeParser), static code =>
        {
            if (!MemberAccessExpressionSyntaxNodeParser.TryReadOrInner(code, out var result))
                return null;

            while (true)
            {
                if (!TokenNodeParser.TryReadExact(code, "+", out var plusToken))
                    return result;

                if (!MemberAccessExpressionSyntaxNodeParser.TryReadOrInner(code, out var right))
                    right = ExpressionSyntaxNodeParser.Missing("Missing expression after '+'.");

                result = new AdditionExpressionSyntaxNodeBuilder(result, plusToken, right);
            }
        });
    }
}