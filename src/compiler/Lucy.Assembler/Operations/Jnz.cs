using Lucy.Assembler.Infrastructure;
using System;

namespace Lucy.Assembler.Operations
{
    /// <summary>
    /// Jump if not zero
    /// </summary>
    public record Jnz(Immediate Offset) : Operation
    {
        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("jnz", Offset);
            w.WriteComment(Comment);
            w.WriteNewLine();
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
