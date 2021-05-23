using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using Lucy.Core.Compiler.Assembling.Infrastructure;

namespace Disassembler.Assembling.Operations
{
    public record Leave() : Operation
    {
        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("leave");
            w.WriteComment(Comment);
            w.WriteNewLine();
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new System.NotImplementedException();
        }
    }
}
