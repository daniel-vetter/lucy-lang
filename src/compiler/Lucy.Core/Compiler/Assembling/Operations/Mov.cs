using Disassembler.Assembling.Infrastructure;
using Disassembler.Assembling.Model;

namespace Disassembler.Assembling.Operations
{
    public record Mov(Operand Target, Operand Source) : Operation
    {
        public override string ToString() => $"mov {Target},{Source}";

        public override void Write(MachineCodeWriter w)
        {
            var op1IsValid = w.ValidateOperand(Source);
            var op2IsValid = w.ValidateOperand(Target);
            if (!op1IsValid || !op2IsValid)
                return;

            w.WriteOperandSizePrefixIfRequired(Source);
            w.WriteAddressSizePrefixIfRequired(Source, Target);

            if (RegisterToRegisterOrDereference(w))
                return;

            if (RegisterOrDereferenceToRegister(w))
                return;

            if (ImmediateToRegister(w))
                return;

            if (ImmediateToRegisterOrDereference(w))
                return;

            w.ReportError("Invalid combination of opcode and operand size.");
        }

        private bool ImmediateToRegisterOrDereference(MachineCodeWriter w)
        {
            if (Target.IsRegisterOrDereference8(out var rm8) && Source.IsImmediate8(out var imm8))
            {
                w.WriteByte(0xC6);
                w.Encode(rm8, 0);
                w.WriteByte(imm8.Value);
                return true;
            }

            if (Target.IsRegisterOrDereference16(out var rm16) && Source.IsImmediate16(out var imm16))
            {
                w.WriteByte(0xC7);
                w.Encode(rm16, 0);
                w.WriteUInt16(imm16.Value);
                return true;
            }

            if (Target.IsRegisterOrDereference32(out var rm32) && Source.IsImmediate32(out var imm32_1))
            {
                w.WriteByte(0xC7);
                w.Encode(rm32, 0);
                w.WriteUInt32(imm32_1.Value);
                return true;
            }

            if (Target.IsRegisterOrDereference64(out var rm64) && Source.IsImmediate32(out var imm32_2))
            {
                w.WriteByte(0b01001000);
                w.WriteByte(0xC7);
                w.Encode(rm64, 0);
                w.WriteUInt32(imm32_2.Value);
                return true;
            }
            return false;
        }

        private bool ImmediateToRegister(MachineCodeWriter w)
        {
            if (Target.IsRegister8(out var r8) && Source.IsImmediate8(out var imm8))
            {
                w.WriteByte((byte)(0xB0 + r8.Index));
                w.WriteByte((byte)imm8.Value, imm8.Annotation);
                return true;
            }

            if (Target.IsRegister16(out var r16) && Source.IsImmediate16(out var imm16))
            {
                w.WriteByte((byte)(0xB8 + r16.Index));
                w.WriteUInt16((ushort)imm16.Value, imm16.Annotation);
                return true;
            }

            if (Target.IsRegister32(out var r32) && Source.IsImmediate32(out var imm32))
            {
                w.WriteByte((byte)(0xB8 + r32.Index));
                w.WriteUInt32(imm32.Value, imm32.Annotation);
                return true;
            }

            if (Target.IsRegister64(out var r64) && Source.IsImmediate64(out var imm64))
            {
                w.WriteByte(0b01001000);
                w.WriteByte((byte)(0xB8 + r64.Index));
                w.WriteUInt32(imm64.Value, imm64.Annotation);
                return true;
            }

            return false;
        }

        private bool RegisterOrDereferenceToRegister(MachineCodeWriter w)
        {
            //The special cases
            if (w.DefaultOperandSize != OperandSize.S64)
            {
                if (Source.IsDereference8(out var m8, isAbsolute: true) && Target.IsRegister8(out var _, index: 0))
                {
                    w.WriteByte(0xA0);
                    w.WriteDisplacment(m8.Displacement, m8.Annotation);
                    return true;
                }

                if (Source.IsDereference16(out var m16, isAbsolute: true) && Target.IsRegister16(out var _, index: 0))
                {
                    w.WriteByte(0xA1);
                    w.WriteDisplacment(m16.Displacement, m16.Annotation);
                    return true;
                }

                if (Source.IsDereference32(out var m32, isAbsolute: true) && Target.IsRegister32(out var _, index: 0))
                {
                    w.WriteByte(0xA1);
                    w.WriteDisplacment(m32.Displacement, m32.Annotation); //Should be offset from Segement offset, whatever a segement is
                    return true;
                }
            }

            //The normal cases
            if (Target.IsRegister8(out var r8) && Source.IsRegisterOrDereference8(out var rm8))
            {
                w.WriteByte(0x8A);
                w.Encode(rm8, r8.Index);
                return true;
            }

            if (Target.IsRegister16(out var r16) && Source.IsRegisterOrDereference16(out var rm16))
            {
                w.WriteByte(0x8B);
                w.Encode(rm16, r16.Index);
                return true;
            }

            if (Target.IsRegister32(out var r32) && Source.IsRegisterOrDereference32(out var rm32))
            {
                w.WriteByte(0x8B);
                w.Encode(rm32, r32.Index);
                return true;
            }

            if (Target.IsRegister64(out var r64) && Source.IsRegisterOrDereference64(out var rm64))
            {
                w.WriteByte(0b01001000);
                w.WriteByte(0x8B);
                w.Encode(rm64, r64.Index);
                return true;
            }

            return false;
        }

        private bool RegisterToRegisterOrDereference(MachineCodeWriter w)
        {
            //The special cases
            if (w.DefaultOperandSize != OperandSize.S64)
            {
                if (Target.IsDereference8(out var m8, isAbsolute: true) && Source.IsRegister8(out var _, index: 0))
                {
                    w.WriteByte(0xA2);
                    w.WriteDisplacment(m8.Displacement, m8.Annotation); //Should be offset from Segement offset, whatever a segement is;
                    return true;
                }

                if (Target.IsDereference16(out var m16, isAbsolute: true) && Source.IsRegister16(out var _, index: 0))
                {
                    w.WriteByte(0xA3);
                    w.WriteDisplacment(m16.Displacement, m16.Annotation); //Should be offset from Segement offset, whatever a segement is;
                    return true;
                }

                if (Target.IsDereference32(out var m32, isAbsolute: true) && Source.IsRegister32(out var _, index: 0))
                {
                    w.WriteByte(0xA3);
                    w.WriteDisplacment(m32.Displacement, m32.Annotation); //Should be offset from Segement offset, whatever a segement is; Warning if value does not fit into a short
                    return true;
                }
            }

            //The normal cases
            if (Target.IsRegisterOrDereference8(out var rm8) && Source.IsRegister8(out var r8))
            {
                w.WriteByte(0x88);
                w.Encode(rm8, r8.Index);
                return true;
            }

            if (Target.IsRegisterOrDereference16(out var rm16) && Source.IsRegister16(out var r16))
            {
                w.WriteByte(0x89);
                w.Encode(rm16, r16.Index);
                return true;
            }

            if (Target.IsRegisterOrDereference32(out var rm32) && Source.IsRegister32(out var r32))
            {
                w.WriteByte(0x89);
                w.Encode(rm32, r32.Index);
                return true;
            }
            if (Target.IsRegisterOrDereference64(out var rm64) && Source.IsRegister64(out var r64))
            {
                w.WriteByte(0b01001000);
                w.WriteByte(0x89);
                w.Encode(rm64, r64.Index);
                return true;
            }

            return false;
        }

    }
}