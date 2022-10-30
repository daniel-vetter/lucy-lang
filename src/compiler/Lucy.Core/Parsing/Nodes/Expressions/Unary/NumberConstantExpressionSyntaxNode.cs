using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing.Nodes.Trivia;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary
{
    public record NumberConstantExpressionSyntaxNode(double Value, SyntaxElement Token) : ExpressionSyntaxNode
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out NumberConstantExpressionSyntaxNode? result)
        {
            var start = code.Position;
            var leadingTrivia = TriviaNode.ReadList(code);

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

        private static NumberConstantExpressionSyntaxNode CreateNode(Code code, ComparableReadOnlyList<TriviaNode> leadingTrivia, int count)
        {
            var str = code.Read(count);
            var num = double.Parse(str, CultureInfo.InvariantCulture);
            var token = new SyntaxElement(leadingTrivia, new TokenNode(str));
            return new NumberConstantExpressionSyntaxNode(num, token);
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
