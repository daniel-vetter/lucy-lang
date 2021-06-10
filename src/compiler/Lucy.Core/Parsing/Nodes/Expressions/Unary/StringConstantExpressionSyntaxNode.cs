using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing.Nodes.Trivia;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary
{
    public class StringConstantExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public StringConstantExpressionSyntaxNode(string value, SyntaxElement str)
        {
            Value = value;
            String = str;
        }

        public string Value { get; }
        public SyntaxElement String { get; }

        public static bool TryRead(Code code, [NotNullWhen(true)] out StringConstantExpressionSyntaxNode? result)
        {
            var start = code.Position;
            var leadingTrivia = TriviaNode.ReadList(code);

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
                    code.ReportError("Unterminated string detected. Missing '\"'");
                    var str = code.Read(len);
                    var token = new SyntaxElement(leadingTrivia, new TokenNode(str));
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
