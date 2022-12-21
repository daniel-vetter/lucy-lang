using Lucy.Core.Parsing.Nodes.Trivia;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;
using System.Collections.Immutable;
using Lucy.Core.Parsing.Nodes.Token;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public static class StringConstantExpressionSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out StringConstantExpressionSyntaxNode? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static StringConstantExpressionSyntaxNode? TryRead(Reader reader)
    {
        return reader.WithCache(nameof(StringConstantExpressionSyntaxNodeParser), static r =>
        {
            if (r.Peek() != '\"')
                return null;

            var len = 1;
            while (true)
            {
                if (r.Peek(len) == '\0')
                {
                    var str = r.Read(len);
                    var trailingTrivia = TriviaParser.Read(r);
                    var token = new TokenNode(null, str, trailingTrivia, ImmutableArray.Create("Unterminated string detected. Missing '\"'"));
                    return StringConstantExpressionSyntaxNode.Create(str[1..], token);
                }

                if (r.Peek(len) == '"')
                {
                    var str = r.Read(len + 1);
                    var trailingTrivia = TriviaParser.Read(r);
                    var token = TokenNode.Create(str, trailingTrivia);
                    return StringConstantExpressionSyntaxNode.Create(str.Substring(1, str.Length - 2), token);
                }

                len++;
            }
        });
    }

    public static StringConstantExpressionSyntaxNode Missing(Reader reader, string? errorMessage = null)
    {
        return StringConstantExpressionSyntaxNode.Create("", TokenNodeParser.Missing(errorMessage));
    }
}