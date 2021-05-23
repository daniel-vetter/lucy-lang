using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;
using Lucy.Core.Compiler.Assembling.Infrastructure;

namespace Disassembler.Assembling.Operations
{
    public record Push(Operand Operand) : Operation
    {
        public override void Write(AssemblyWriter w)
        {
            w.WritePadding();
            w.WriteOperation("push", Operand);
            w.WriteComment(Comment);
            w.WriteNewLine();
        }

        public override void Write(MachineCodeWriter w)
        {
            if (!w.ValidateOperand(Operand))
                return;

            w.WriteOperandSizePrefixIfRequired(Operand);
            w.WriteAddressSizePrefixIfRequired(Operand);

            if (Operand.IsImmediate8(out var im8))
            {
                w.WriteByte(0x6A);
                w.WriteByte((byte)im8.Value);
                return;
            }

            if (Operand.IsImmediate16(out var im16))
            {
                w.WriteByte(0x68);
                w.WriteUInt16((ushort)im16.Value);
                return;
            }

            if (Operand.IsImmediate32(out var im32))
            {
                w.WriteByte(0x68);
                w.WriteUInt32(im32.Value);
                return;
            }

            if (Operand is Register r16 && r16.Size == OperandSize.S16)
            {
                w.WriteByte((byte)(0x50 + r16.Index));
                return;
            }

            if (Operand is Register r32 && r32.Size == OperandSize.S32)
            {
                if (w.DefaultOperandSize == OperandSize.S64)
                {
                    w.ReportError("Instruction not supported in 64 bit mode.");
                    return;
                }

                w.WriteByte((byte)(0x50 + r32.Index));
                return;
            }

            if (Operand is Register r64 && r64.Size == OperandSize.S64)
            {
                w.WriteByte((byte)(0x50 + r64.Index));
                return;
            }

            if (Operand is Memory deref16 && deref16.Size == OperandSize.S16)
            {
                w.WriteByte(0xFF);
                w.Encode(deref16, 6);
                return;
            }

            if (Operand is Memory deref32 && deref32.Size == OperandSize.S32)
            {
                if (w.DefaultOperandSize == OperandSize.S64)
                {
                    w.ReportError("Instruction not supported in 64 bit mode.");
                    return;
                }

                w.WriteByte(0xFF);
                w.Encode(deref32, 6);
                return;
            }

            if (Operand is Memory deref64 && deref64.Size == OperandSize.S64)
            {
                w.WriteByte(0xFF);
                w.Encode(deref64, 6);
                return;
            }

            w.ReportError("Invalid combination of opcode and operand size.");
        }
    }
}
