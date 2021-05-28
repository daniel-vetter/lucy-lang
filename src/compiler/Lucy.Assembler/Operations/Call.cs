using Lucy.Assembler.Infrastructure;

namespace Lucy.Assembler.Operations
{
    public record Call(IOperand Operand) : Operation
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

            /*
            if (Operand.IsImmediate32(out var imm32))
            {
                return;
            }
            */
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
