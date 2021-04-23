using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using System;

namespace Disassembler.Assembling.Operations
{
    public record Add(Operand TargetRegister, Operand ValueToAdd) : Operation
    {
        public override string ToString()
        {
            return $"add {TargetRegister},{ValueToAdd}";
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
