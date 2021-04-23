using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;

namespace Disassembler.Assembling.Operations
{
    public record Nop() : Operation
    {
        public override string ToString()
        {
            return $"nop";
        }

        public override void Write(MachineCodeWriter w)
        {
            w.WriteByte(0x90);
        }
    }
}
