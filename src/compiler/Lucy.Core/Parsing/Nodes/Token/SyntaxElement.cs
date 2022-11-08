using Lucy.Core.Parsing.Nodes.Trivia;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Token
{
    public class SyntaxElementParser
    {
        public static SyntaxElement Missing(string? errorMessage = null) => new SyntaxElement(new List<TriviaNode>(), TokenNodeParser.Missing(errorMessage));

        public static bool TryReadExact(Code code, string text, [NotNullWhen(true)] out SyntaxElement? result)
        {
            var start = code.Position;
            var leadingTrivia = TriviaNodeParser.ReadList(code);

            for (int i = 0; i < text.Length; i++)
                if (code.Peek(i) != text[i])
                {
                    code.SeekTo(start);
                    result = null;
                    return false;
                }


            result = new SyntaxElement(leadingTrivia, new TokenNode(text));
            code.Seek(text.Length);
            return true;
        }

        public static bool TryReadIdentifier(Code code, [NotNullWhen(true)] out SyntaxElement? result)
        {
            var start = code.Position;
            var trivia = TriviaNodeParser.ReadList(code);

            int length = 0;
            while (IsIdentifierChar(code.Peek(length), length == 0))
                length++;

            if (length == 0)
            {
                code.SeekTo(start);
                result = null;
                return false;
            }

            result = new SyntaxElement(trivia, new TokenNode(code.Read(length)));
            return true;
        }

        public static bool TryReadKeyword(Code code, string keyword, [NotNullWhen(true)] out SyntaxElement? result)
        {
            var start = code.Position;
            if (!TryReadIdentifier(code, out result) || result.Token.Text != keyword)
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

