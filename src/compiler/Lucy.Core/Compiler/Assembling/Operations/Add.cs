using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using Lucy.Core.Compiler.Assembling.Infrastructure;
using System;

namespace Disassembler.Assembling.Operations
{
    public record Add(Operand TargetRegister, Operand ValueToAdd) : Operation
    {
        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }

        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("add", TargetRegister, ValueToAdd);
            w.WriteComment(Comment);
            w.WriteNewLine();
        }
    }
}
