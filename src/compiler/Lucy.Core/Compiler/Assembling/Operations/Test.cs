using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using Lucy.Core.Compiler.Assembling.Infrastructure;
using System;

namespace Disassembler.Assembling.Operations
{
    public record Test(Operand Op1, Operand Op2) : Operation
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
