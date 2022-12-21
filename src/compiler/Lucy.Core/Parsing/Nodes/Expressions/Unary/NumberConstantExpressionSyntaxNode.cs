using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Trivia;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public static class NumberConstantExpressionSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out NumberConstantExpressionSyntaxNode? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static NumberConstantExpressionSyntaxNode? TryRead(Reader reader)
    {
        return reader.WithCache(nameof(NumberConstantExpressionSyntaxNodeParser), static code =>
        {
            var negative = false;
            if (code.Peek() == '-')
            {
                code.Read(1);
                negative = true;
            }

            if (!CountDigits(code, out var beforeDigitCount))
                return null;

            if (code.Peek(beforeDigitCount) != '.')
                return CreateNode(code, TriviaParser.Read(code), beforeDigitCount, negative);

            if (!CountDigits(code, out var afterDigitCount))
                return CreateNode(code, TriviaParser.Read(code), beforeDigitCount, negative);

            return CreateNode(code, TriviaParser.Read(code), beforeDigitCount + 1 + afterDigitCount, negative);
        });
    }

    private static NumberConstantExpressionSyntaxNode CreateNode(Reader reader, string? trailingTrivia, int count, bool negative)
    {
        var str = reader.Read(count);
        var num = double.Parse(str, CultureInfo.InvariantCulture);
        if (negative) num *= -1;
        var token = TokenNode.Create((negative ? "-" : "") + str, trailingTrivia);
        return NumberConstantExpressionSyntaxNode.Create(num, token);
    }

    private static bool CountDigits(Reader reader, out int count)
    {
        count = 0;
        while (true)
        {
            var ch = reader.Peek(count);
            if (ch is < '0' or > '9')
                break;
            count++;
        }
        return count != 0;
    }
}