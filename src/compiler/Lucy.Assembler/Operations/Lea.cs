using Lucy.Assembler.Infrastructure;

namespace Lucy.Assembler.Operations
{
    public record Lea(Register Target, Memory Source) : Operation
    {
        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("lea", Target, Source);
            w.WriteComment(Comment);
            w.WriteNewLine();
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new System.NotImplementedException();
        }
    }
}
