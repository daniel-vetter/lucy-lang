using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using System;

namespace Disassembler.Assembling.Operations
{
    public record Cmp(Operand TargetRegister, Operand ValueToCmp) : Operation
    {
        public override string ToString()
        {
            return $"cmp {TargetRegister},{ValueToCmp}";
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
