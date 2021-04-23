using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using System;

namespace Disassembler.Assembling.Operations
{
    public record Test(Operand Op1, Operand Op2) : Operation
    {
        public override string ToString()
        {
            return $"test {Op1},{Op2}";
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
