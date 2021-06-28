using System.Diagnostics.CodeAnalysis;

namespace Lucy.Assembler.Parsing.Reader
{
    public static class LabelReader
    {
        public static bool TryReadLabelRef(this AsmReader reader, [NotNullWhen(true)] out LabelRef? result)
        {
            if (!reader.TryReadOperandSize(out var size))
            {
                result = null;
                return false;
            }

            if (reader.TryReadIdentifier(out var identifier))
            {
                result = new LabelRef(size, identifier);
                return true;
            }

            result = null;
            return false;
        }

        public static bool TryReadLabel(this AsmReader reader, [NotNullWhen(true)] out Label? result)
        {
            using var t = reader.BeginTransaction();

            if (!reader.TryReadIdentifier(out var identifier))
            {
                result = null;
                return false;
            }

            if (reader.Read() != ':')
            {
                result = null;
                return false;
            }

            t.Commit();
            result = new Label(identifier);
            return true;
        }
    }
}
