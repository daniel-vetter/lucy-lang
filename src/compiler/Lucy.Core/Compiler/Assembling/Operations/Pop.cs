using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using System;

namespace Disassembler.Assembling.Operations
{
    public record Pop(Operand Operand) : Operation
    {
        public override string ToString() => $"pop {Operand}";

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
