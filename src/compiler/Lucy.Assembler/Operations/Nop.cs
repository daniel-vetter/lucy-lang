using Lucy.Assembler.Infrastructure;

namespace Lucy.Assembler.Operations
{
    public record Nop() : Operation
    {
        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("nop");
            w.WriteComment(Comment);
            w.WriteNewLine();
        }

        public override void Write(MachineCodeWriter w)
        {
            w.WriteByte(0x90);
        }
    }
}
