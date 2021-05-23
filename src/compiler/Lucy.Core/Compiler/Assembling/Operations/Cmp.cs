using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using Lucy.Core.Compiler.Assembling.Infrastructure;
using System;

namespace Disassembler.Assembling.Operations
{
    public record Cmp(Operand TargetRegister, Operand ValueToCmp) : Operation
    {
        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }

        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("cmp", TargetRegister, ValueToCmp);
            w.WriteComment(Comment);
            w.WriteNewLine();
        }
    }
}
