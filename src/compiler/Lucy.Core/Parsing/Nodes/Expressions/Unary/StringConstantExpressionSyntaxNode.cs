using Lucy.Core.Parsing.Nodes.Trivia;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary
{
    public class StringConstantExpressionSyntaxNodeParser
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out StringConstantExpressionSyntaxNode? result)
        {
            var start = code.Position;
            var leadingTrivia = TriviaNodeParser.ReadList(code);

            if (code.Peek() != '\"')
            {
                code.SeekTo(start);
                result = null;
                return false;
            }

            int len = 1;
            while (true)
            {
                if (code.Peek(len) == '\0')
                {
                    var str = code.Read(len);
                    var token = new SyntaxElement(leadingTrivia, new TokenNode(str));
                    token.SyntaxErrors.Add("Unterminated string detected. Missing '\"'");
                    result = new StringConstantExpressionSyntaxNode(str.Substring(1), token);
                    return true;
                }

                if (code.Peek(len) == '"')
                {
                    var str = code.Read(len + 1);
                    var token = new SyntaxElement(leadingTrivia, new TokenNode(str));
                    result = new StringConstantExpressionSyntaxNode(str.Substring(1, str.Length - 2), token);
                    return true;
                }

                len++;
            }
        }
    }
}
