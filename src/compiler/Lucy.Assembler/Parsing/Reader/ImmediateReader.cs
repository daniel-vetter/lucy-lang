using System.Diagnostics.CodeAnalysis;

namespace Lucy.Assembler.Parsing.Reader
{
    public static class ImmediateReader
    {
        public static bool TryReadImmediate(this AsmReader reader, [NotNullWhen(true)] out Immediate? immediate)
        {
            using var t = reader.BeginTransaction();

            reader.TryReadSign(out var sign);
            if (!reader.TryReadNumber(out var number))
            {
                immediate = null;
                return false;
            }

            if (sign == Sign.Minus)
                number = 0 - number;

            t.Commit();
            immediate = new Immediate(reader.DefaultOperandSize, number);
            return true;
        }

        public static bool TryReadSign(this AsmReader reader, [NotNullWhen(true)] out Sign? sign)
        {
            using var t = reader.BeginTransaction();
            reader.TryReadWhitespace();

            var ch = reader.Read();
            if (ch == '-')
            {
                t.Commit();
                sign = Sign.Minus;
                return true;
            }

            if (ch == '+')
            {
                t.Commit();
                sign = Sign.Plus;
                return true;
            }

            sign = null;
            return false;
        }
    }

    public enum Sign
    {
        Plus,
        Minus
    }
}
