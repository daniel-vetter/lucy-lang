using System.Diagnostics.CodeAnalysis;

namespace Lucy.Assembler.Parsing.Reader
{
    public static class WhitespaceReader
    {
        public static bool TryReadWhitespace(this AsmReader reader) => reader.TryReadWhitespace(out var _);
        public static bool TryReadWhitespace(this AsmReader reader, [NotNullWhen(true)] out string? result)
        {
            int len = 0;
            while (true)
            {
                var ch = reader.Peek(len);
                if (ch != '\r' && ch != '\n' && ch != ' ' && ch != '\t')
                    break;
                len++;
            }

            if (len == 0)
            {
                result = null;
                return false;
            }

            result = reader.Read(len);
            return true;
        }
    }
}
