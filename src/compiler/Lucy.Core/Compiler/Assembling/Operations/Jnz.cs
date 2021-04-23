using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using System;

namespace Disassembler.Assembling.Operations
{
    /// <summary>
    /// Jump if not zero
    /// </summary>
    public record Jnz(Immediate Offset) : Operation
    {
        public override string ToString()
        {
            return $"jnz {Offset}";
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new NotImplementedException();
        }
    }
}
