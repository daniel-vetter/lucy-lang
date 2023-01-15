using Lucy.Core.Model;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Parsing.Nodes.Stuff;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public static class FunctionCallExpressionSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out FunctionCallExpressionSyntaxNode? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static FunctionCallExpressionSyntaxNode? TryRead(Reader reader)
    {
        return reader.WithCache(nameof(FunctionCallExpressionSyntaxNodeParser), static (r, _) =>
        {
            if (!TokenNodeParser.TryReadIdentifier(r, out var functionName))
                return null;

            if (!TokenNodeParser.TryReadExact(r, "(", out var openBracket))
                return null;

            var argumentList = FunctionCallArgumentSyntaxNodeParser.Read(r);

            if (!TokenNodeParser.TryReadExact(r, ")", out var closeBracket))
                closeBracket = TokenNodeParser.Missing("Expected ')'.");

            return FunctionCallExpressionSyntaxNode.Create(functionName, openBracket, argumentList, closeBracket);
        });
    }
}