using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using System;

namespace Disassembler.Assembling.Operations
{
    /// <summary>
    /// Jump if zero
    /// </summary>
    public record Jz(Immediate Offset) : Operation
    {
        public override string ToString()
        {
            return $"Jz {Offset}";
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
