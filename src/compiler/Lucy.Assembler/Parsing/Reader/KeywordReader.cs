namespace Lucy.Assembler.Parsing.Reader
{
    public static class KeywordReader
    {
        public static bool TryReadKeyword(this AsmReader reader, string keyword)
        {
            using var t = reader.BeginTransaction();
            if (reader.TryReadIdentifier(out var identifier) && identifier == keyword)
            {
                t.Commit();
                return true;
            }

            return false;
        }
    }
}
