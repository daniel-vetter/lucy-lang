using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using Lucy.Core.Compiler.Assembling.Infrastructure;
using System;

namespace Disassembler.Assembling.Operations
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
