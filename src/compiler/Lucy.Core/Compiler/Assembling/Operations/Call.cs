using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using Lucy.Core.Compiler.Assembling.Infrastructure;

namespace Disassembler.Assembling.Operations
{
    public record Call(Operand Operand) : Operation
    {
        public override string ToString() => $"call {Operand}";

        public override void Write(MachineCodeWriter w)
        {
            if (Operand.IsRegisterOrDereference32(out var rm32))
            {
                w.WriteByte(0xFF);
                w.Encode(rm32, 2);
                return;
            }

            w.ReportError("Invalid combination of opcode and operand size.");
        }

        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("call", Operand);
            w.WriteComment(Comment);
            w.WriteNewLine();
        }
    }
}
