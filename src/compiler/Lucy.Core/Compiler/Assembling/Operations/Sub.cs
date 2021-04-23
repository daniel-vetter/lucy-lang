using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using System;

namespace Disassembler.Assembling.Operations
{
    public record Sub(Operand TargetRegister, Operand ValueToSubstract) : Operation
    {
        public override string ToString()
        {
            return $"sub {TargetRegister},{ValueToSubstract}";
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
