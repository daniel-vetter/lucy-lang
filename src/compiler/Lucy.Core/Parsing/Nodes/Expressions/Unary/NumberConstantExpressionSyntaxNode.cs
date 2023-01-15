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
        return reader.WithCache(nameof(NumberConstantExpressionSyntaxNodeParser), static (r, _) =>
        {
            var negative = false;
            if (r.Peek() == '-')
            {
                r.Seek(1);
                negative = true;
            }

            if (!CountDigits(r, out var beforeDigitCount))
                return null;

            if (r.Peek(beforeDigitCount) != '.')
                return CreateNode(r, beforeDigitCount, negative);

            if (!CountDigits(r, out var afterDigitCount))
                return CreateNode(r, beforeDigitCount, negative);

            return CreateNode(r, beforeDigitCount + 1 + afterDigitCount, negative);
        });
    }

    private static NumberConstantExpressionSyntaxNode CreateNode(Reader reader, int count, bool negative)
    {
        var str = reader.Read(count);
        var num = double.Parse(str, CultureInfo.InvariantCulture);
        if (negative) num *= -1;
        var token = TokenNode.Create((negative ? "-" : "") + str, TriviaParser.Read(reader));
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