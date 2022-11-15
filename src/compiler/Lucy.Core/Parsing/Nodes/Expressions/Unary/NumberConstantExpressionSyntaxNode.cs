using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Trivia;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary
{
    public class NumberConstantExpressionSyntaxNodeParser
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out NumberConstantExpressionSyntaxNodeBuilder? result)
        {
            var start = code.Position;
            var leadingTrivia = TriviaNodeParser.ReadList(code);

            if (!CountDigits(code, out var beforeDigitCount))
            {
                code.SeekTo(start);
                result = null;
                return false;
            }

            if (code.Peek(beforeDigitCount) != '.')
            {
                result = CreateNode(code, leadingTrivia, beforeDigitCount);
                return true;
            }
            
            if (!CountDigits(code, out var afterDigitCount))
            {
                result = CreateNode(code, leadingTrivia, beforeDigitCount);
                return true;
            }

            result = CreateNode(code, leadingTrivia, beforeDigitCount + 1 + afterDigitCount);
            return true;
        }

        private static NumberConstantExpressionSyntaxNodeBuilder CreateNode(Code code, List<TriviaNodeBuilder> leadingTrivia, int count)
        {
            var str = code.Read(count);
            var num = double.Parse(str, CultureInfo.InvariantCulture);
            var token = new SyntaxElementBuilder(leadingTrivia, new TokenNodeBuilder(str));
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
}
