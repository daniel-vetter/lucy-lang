using Lucy.Assembler.Infrastructure;
using System;

namespace Lucy.Assembler.Operations
{
    /// <summary>
    /// Jump if zero
    /// </summary>
    public record Jz(Immediate Offset) : Operation
    {
        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("jz", Offset);
            w.WriteComment(Comment);
            w.WriteNewLine();
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
