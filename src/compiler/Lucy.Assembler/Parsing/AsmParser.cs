using Lucy.Assembler.Parsing.Reader;

namespace Lucy.Assembler.Parsing
{
    public static class AsmParser
    {
        public static AsmModule Parse(string code, OperandSize defaultOperandSize)
        {
            var reader = new AsmReader(code, defaultOperandSize);
            var statements = reader.ReadStatementList();

            var m = new AsmModule();
            m.Stataments.AddRange(statements);
            return m;
        }
    }
}
