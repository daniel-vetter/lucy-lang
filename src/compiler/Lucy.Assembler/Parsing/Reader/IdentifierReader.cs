using System.Diagnostics.CodeAnalysis;

namespace Lucy.Assembler.Parsing.Reader
{
    public static class IdentifierReader
    {
        public static bool TryReadIdentifier(this AsmReader reader, [NotNullWhen(true)] out string? identifier)
        {
            using var t = reader.BeginTransaction();

            reader.TryReadWhitespace();

            int length = 0;
            while (IsIdentifierChar(reader.Peek(length), length == 0))
                length++;

            if (length == 0)
            {
                identifier = null;
                return false;
            }

            t.Commit();
            identifier = reader.Read(length);
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

        public static bool TryReadExact(this AsmReader reader, string text)
        {
            using var t = reader.BeginTransaction();
            reader.TryReadWhitespace();

            for (int i = 0; i < text.Length; i++)
                if (reader.Peek(i) != text[i])
                    return false;

            reader.Seek(text.Length);
            t.Commit();
            return true;
        }
    }
}
