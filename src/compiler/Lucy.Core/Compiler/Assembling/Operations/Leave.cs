using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;

namespace Disassembler.Assembling.Operations
{
    public record Leave() : Operation
    {
        public override string ToString()
        {
            return $"leave";
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new System.NotImplementedException();
        }
    }
}
