using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Stuff;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

public static class AdditionExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
    {
        result = TryReadOrInner(reader);
        return result != null;
    }

    public static ExpressionSyntaxNode? TryReadOrInner(Reader reader)
    {
        return reader.WithCache(nameof(AdditionExpressionSyntaxNodeParser), static (r, _) =>
        {
            if (!MemberAccessExpressionSyntaxNodeParser.TryReadOrInner(r, out var result))
                return null;

            while (true)
            {
                if (!TokenNodeParser.TryReadExact(r, "+", out var plusToken))
                    return result;

                if (!MemberAccessExpressionSyntaxNodeParser.TryReadOrInner(r, out var right))
                    right = ExpressionSyntaxNodeParser.Missing("Missing expression after '+'.");

                result = AdditionExpressionSyntaxNode.Create(result, plusToken, right);
            }
        });
    }
}