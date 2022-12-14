using Lucy.Core.Parsing.Nodes.Trivia;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public static class StringConstantExpressionSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out StringConstantExpressionSyntaxNodeBuilder? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static StringConstantExpressionSyntaxNodeBuilder? TryRead(Reader reader)
    {
        return reader.WithCache(nameof(StringConstantExpressionSyntaxNodeParser), static code =>
        {
            if (code.Peek() != '\"')
                return null;

            var len = 1;
            while (true)
            {
                if (code.Peek(len) == '\0')
                {
                    var str = code.Read(len);
                    var trailingTrivia = TriviaParser.Read(code);
                    var token = new TokenNodeBuilder(str, trailingTrivia)
                    {
                        SyntaxErrors = new() { "Unterminated string detected. Missing '\"'" }
                    };
                    return new StringConstantExpressionSyntaxNodeBuilder(str[1..], token);
                }

                if (code.Peek(len) == '"')
                {
                    var str = code.Read(len + 1);
                    var trailingTrivia = TriviaParser.Read(code);
                    var token = new TokenNodeBuilder(str, trailingTrivia);
                    return new StringConstantExpressionSyntaxNodeBuilder(str.Substring(1, str.Length - 2), token);
                }

                len++;
            }
        });
    }

    public static StringConstantExpressionSyntaxNodeBuilder Missing(string? errorMessage = null)
    {
        var node = new StringConstantExpressionSyntaxNodeBuilder("", TokenNodeParser.Missing(errorMessage));
        return node;
    }
}