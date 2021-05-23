using Lucy.Assembler.Infrastructure;
using System;

namespace Lucy.Assembler.Operations
{
    public record Cmp(IOperand TargetRegister, IOperand ValueToCmp) : Operation
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
