using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using Lucy.Core.Compiler.Assembling.Infrastructure;

namespace Disassembler.Assembling.Operations
{
    public record Ret(Immediate? Imm = null) : Operation
    {
        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            if (Imm == null)
                w.WriteOperation("ret");
            else
                w.WriteOperation("ret", Imm);
            w.WriteComment(Comment);
            w.WriteNewLine();
        }

        public override void Write(MachineCodeWriter w)
        {
            throw new System.NotImplementedException();
        }
    }
}
