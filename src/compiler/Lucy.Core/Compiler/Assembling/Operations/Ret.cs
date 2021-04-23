using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;

namespace Disassembler.Assembling.Operations
{
    public record Ret(Immediate? Imm = null) : Operation
    {
        public override string ToString()
        {
            if (Imm == null) return $"ret";
            else return $"ret {Imm}";
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new System.NotImplementedException();
        }
    }
}
