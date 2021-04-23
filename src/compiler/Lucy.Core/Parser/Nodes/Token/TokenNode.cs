using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Trivia;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Token
{
    public class TokenNode : SyntaxNode
    {
        public TokenNode(TriviaListNode leadingTrivia, string value)
        {
            LeadingTrivia = leadingTrivia;
            Value = value;
        }

        public TriviaListNode LeadingTrivia { get; set; }
        public string Value { get; set; }

        public static bool TryReadExact(Code code, string text, [NotNullWhen(true)] out TokenNode? result)
        {
            var start = code.Position;
            var leadingTrivia = TriviaListNode.Read(code);

            for (int i = 0; i < text.Length; i++)
                if (code.Peek(i) != text[i])
                {
                    code.SeekTo(start);
                    result = null;
                    return false;
                }


            result = new TokenNode(leadingTrivia, text);
            code.Seek(text.Length);
            return true;
        }

        public static bool TryReadIdentifier(Code code, [NotNullWhen(true)] out TokenNode? result)
        {
            var start = code.Position;
            var trivia = TriviaListNode.Read(code);

            int length = 0;
            while (IsIdentifierChar(code.Peek(length), length == 0))
                length++;

            if (length == 0)
            {
                code.SeekTo(start);
                result = null;
                return false;
            }

            result = new TokenNode(trivia, code.Read(length));
            return true;
        }

        public static bool TryReadKeyword(Code code, string keyword, [NotNullWhen(true)] out TokenNode? result)
        {
            var start = code.Position;
            if (!TryReadIdentifier(code, out result) || result.Value != keyword)
            {
                code.SeekTo(start);
                return false;
            }

            return true;
        }

        private static bool IsIdentifierChar(char ch, bool firstChar)
        {
            var isValid = ch >= 'a' && ch <= 'z' ||
                          ch >= 'A' && ch <= 'Z' ||
                          ch == '_';

            if (!isValid && !firstChar)
            {
                if (ch >= '0' && ch <= '9' || ch == '/')
                    isValid = true;
            }

            return isValid;
        }
    }
}

