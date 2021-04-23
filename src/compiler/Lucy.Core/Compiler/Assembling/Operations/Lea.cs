using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;

namespace Disassembler.Assembling.Operations
{
    public record Lea(Register Target, Memory Source) : Operation
    {
        public override string ToString()
        {
            return $"lea {Target},{Source}";
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new System.NotImplementedException();
        }
    }
}
