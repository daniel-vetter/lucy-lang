using System.Diagnostics.CodeAnalysis;

namespace Lucy.Assembler.Parsing.Reader
{
    public static class NumberReader
    {
        public static bool TryReadNumber(this AsmReader reader, out uint result)
        {
            using var t = reader.BeginTransaction();
            reader.TryReadWhitespace();

            int len = 0;
            while ("0123456789".Contains(reader.Peek(len)))
                len++;

            if (len == 0)
            {
                result = default;
                return false;
            }

            t.Commit();
            result = uint.Parse(reader.Read(len));
            return true;
        }
    }
}
