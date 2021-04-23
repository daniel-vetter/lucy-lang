using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using System;

namespace Disassembler.Assembling.Operations
{
    public record Jmp(Operand Operand) : Operation
    {
        public override string ToString()
        {
            return $"jmp {Operand}";
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
