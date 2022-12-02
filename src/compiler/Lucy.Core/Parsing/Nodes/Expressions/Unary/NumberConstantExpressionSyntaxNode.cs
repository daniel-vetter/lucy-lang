using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Trivia;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public static class NumberConstantExpressionSyntaxNodeParser
{
    public static bool TryRead(Code code, [NotNullWhen(true)] out NumberConstantExpressionSyntaxNodeBuilder? result)
    {
        var start = code.Position;
        var leadingTrivia = TriviaNodeParser.ReadList(code);

        var negative = false;
        if (code.Peek() == '-')
        {
            code.Read(1);
            negative = true;
        }

        if (!CountDigits(code, out var beforeDigitCount))
        {
            code.SeekTo(start);
            result = null;
            return false;
        }

        if (code.Peek(beforeDigitCount) != '.')
        {
            result = CreateNode(code, leadingTrivia, beforeDigitCount, negative);
            return true;
        }
            
        if (!CountDigits(code, out var afterDigitCount))
        {
            result = CreateNode(code, leadingTrivia, beforeDigitCount, negative);
            return true;
        }

        result = CreateNode(code, leadingTrivia, beforeDigitCount + 1 + afterDigitCount, negative);
        return true;
    }

    private static NumberConstantExpressionSyntaxNodeBuilder CreateNode(Code code, List<TriviaNodeBuilder> leadingTrivia, int count, bool negative)
    {
        var str = code.Read(count);
        var num = double.Parse(str, CultureInfo.InvariantCulture);
        if (negative) num *= -1;
        var token = new SyntaxElementBuilder(leadingTrivia, new TokenNodeBuilder((negative ? "-" : "") + str));
        return new NumberConstantExpressionSyntaxNodeBuilder(num, token);
    }

    private static bool CountDigits(Code code, out int count)
    {
        count = 0;
        while (true)
        {
            var ch = code.Peek(count);
            if (ch < '0' || ch > '9')
                break;
            count++;
        }
        return count != 0;
    }
}