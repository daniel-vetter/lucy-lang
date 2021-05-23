using Lucy.Assembler.Infrastructure;
using System;

namespace Lucy.Assembler.Operations
{
    public record Test(IOperand Op1, IOperand Op2) : Operation
    {
        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("test", Op1, Op2);
            w.WriteComment(Comment);
            w.WriteNewLine();
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
