using Lucy.Assembler.Infrastructure;
using System;

namespace Lucy.Assembler.Operations
{
    public record Add(IOperand TargetRegister, IOperand ValueToAdd) : Operation
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
