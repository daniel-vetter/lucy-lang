using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using Lucy.Core.Compiler.Assembling.Infrastructure;
using System;

namespace Disassembler.Assembling.Operations
{
    public record Jmp(Operand Operand) : Operation
    {
        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }

        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("jmp", Operand);
            w.WriteComment(Comment);
            w.WriteNewLine();
        }
    }
}
