using Disassembler.Assembling.Infrastructure;

namespace Disassembler.Assembling.Model
{
    public abstract record Operation
    {
        public abstract void Write(MachineCodeWriter w);
    }
}
