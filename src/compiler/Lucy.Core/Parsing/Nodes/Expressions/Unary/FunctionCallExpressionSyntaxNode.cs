using Lucy.Core.Model;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public static class FunctionCallExpressionSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out FunctionCallExpressionSyntaxNodeBuilder? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static FunctionCallExpressionSyntaxNodeBuilder? TryRead(Reader reader)
    {
        return reader.WithCache(nameof(FunctionCallExpressionSyntaxNodeParser), static code =>
        {
            if (!TokenNodeParser.TryReadIdentifier(code, out var functionName))
                return null;

            if (!TokenNodeParser.TryReadExact(code, "(", out var openBracket))
                return null;

            var argumentList = FunctionCallArgumentSyntaxNodeParser.Read(code);

            if (!TokenNodeParser.TryReadExact(code, ")", out var closeBracket))
                closeBracket = TokenNodeParser.Missing("Expected ')'.");

            return new FunctionCallExpressionSyntaxNodeBuilder(functionName, openBracket, argumentList, closeBracket);
        });
    }
}