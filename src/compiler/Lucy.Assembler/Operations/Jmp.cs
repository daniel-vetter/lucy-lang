using Lucy.Assembler.Infrastructure;
using System;

namespace Lucy.Assembler.Operations
{
    public record Jmp(IOperand Operand) : Operation
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
