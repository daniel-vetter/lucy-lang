using Lucy.Assembler.Infrastructure;
using System;

namespace Lucy.Assembler.Operations
{
    public record Sub(IOperand TargetRegister, IOperand ValueToSubstract) : Operation
    {
        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("sub", TargetRegister, ValueToSubstract);
            w.WriteComment(Comment);
            w.WriteNewLine();
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
