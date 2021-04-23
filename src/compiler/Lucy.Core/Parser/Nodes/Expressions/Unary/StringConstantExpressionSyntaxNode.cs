using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Token;
using Lucy.Core.Parser.Nodes.Trivia;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Expressions.Unary
{
    public class StringConstantExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public StringConstantExpressionSyntaxNode(string value, TokenNode token)
        {
            Value = value;
            Token = token;
        }

        public string Value { get; }
        public TokenNode Token { get; }

        public static bool TryRead(Code code, [NotNullWhen(true)] out StringConstantExpressionSyntaxNode? result)
        {
            var start = code.Position;
            var leadingTrivia = TriviaListNode.Read(code);

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
                    var token = new TokenNode(leadingTrivia, str);
                    result = new StringConstantExpressionSyntaxNode(str.Substring(1), token);
                    return true;
                }

                if (code.Peek(len) == '"')
                {
                    var str = code.Read(len + 1);
                    var token = new TokenNode(leadingTrivia, str);
                    result = new StringConstantExpressionSyntaxNode(str.Substring(1, str.Length - 2), token);
                    return true;
                }

                len++;
            }
        }
    }
}
