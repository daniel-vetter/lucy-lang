using Disassembler.Infrastructure.Memory;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Disassembler.Assembling.Infrastructure
{
    public class MachineCodeWriter
    {
        private readonly MemoryBlock _m;
        private readonly List<AssemblerIssue> _issues = new();

        public uint Address
        {
            get => _m.Address;
            set => _m.Address = value;
        }

        public OperandSize DefaultOperandSize { get; }
        public ImmutableArray<AssemblerIssue> Issues => _issues.ToImmutableArray();

        public MachineCodeWriter(OperandSize defaultOperandSize, MemoryBlock memoryBlock)
        {
            _m = memoryBlock;
            DefaultOperandSize = defaultOperandSize;
        }

        public void ReportError(string message) => _issues.Add(new AssemblerIssue(AssemblerIssueSeverity.Error, message));
        public void ReportWarning(string message) => _issues.Add(new AssemblerIssue(AssemblerIssueSeverity.Warning, message));

        public void ValidateOperand(Operand operand, out bool isValid) => isValid = ValidateOperand(operand);
        public bool ValidateOperand(Operand operand)
        {
            if (operand is Memory m)
            {
                if (m.Base != null && m.Index != null && m.Base.Size != m.Index.Size)
                {
                    ReportError($"{m.Base.Size.Bits} bit base register size can not be used with {m.Index.Size.Bits} bit index register size.");
                    return false;
                }

                if (m.AddressSize == OperandSize.S8)
                {
                    ReportError("8 bit address size not supported.");
                    return false;
                }

                if (m.AddressSize == OperandSize.S16)
                {
                    if (m.Scale != Scale.S1)
                    {
                        ReportError("16 bit address mode does not allow for a scale != 1.");
                        return false;
                    }
                }
            }

            if (DefaultOperandSize != OperandSize.S64)
            {
                if (operand.Size == OperandSize.S64)
                {
                    ReportError($"64 bit operand size not supported in {DefaultOperandSize.Bits} bit mode.");
                    return false;
                }

                if (operand is Memory derefA && derefA.AddressSize == OperandSize.S64)
                {
                    ReportError($"64 bit address size not supported in {DefaultOperandSize.Bits} bit mode.");
                    return false;
                }
            }

            if (DefaultOperandSize == OperandSize.S64)
            {
                if (operand is Memory deref3 && deref3.AddressSize == OperandSize.S16)
                {
                    ReportError("16 Bit address size not supported in 64 bit mode.");
                    return false;
                }
            }

            return true;
        }

        public void WriteOperandSizePrefixIfRequired(Operand op)
        {
            if (op.Size == OperandSize.S8)
                return;

            var operandIs16Bit = op.Size == OperandSize.S16;
            var modeIs16Bit = DefaultOperandSize == OperandSize.S16;
            if (operandIs16Bit != modeIs16Bit)
                WriteByte(0x66);
        }

        public void WriteByte(byte @byte, object? annotation = null)
        {
            if (annotation != null)
                _m.AddAnnotation(annotation);
            _m.WriteUInt8(@byte);
        }

        public void WriteByte(uint @byte, object? annotation = null)
        {
            if (@byte > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(@byte));
            if (annotation != null)
                _m.AddAnnotation(annotation);
            _m.WriteUInt8((byte)@byte);
        }

        public void WriteUInt32(uint value, object? annotation = null)
        {
            if (annotation != null)
                _m.AddAnnotation(annotation);
            _m.WriteUInt32(value);
        }

        public void WriteInt32(int value, object? annotation = null)
        {
            if (annotation != null)
                _m.AddAnnotation(annotation);
            _m.WriteInt32(value);
        }

        public void WriteDisplacment(int value, object? annotation = null)
        {
            if (DefaultOperandSize == OperandSize.S16)
            {
                if (value > short.MaxValue)
                    ReportError("Displacement is outside the valid range.");
                WriteInt16((short)value, annotation);
                return;
            }

            if (DefaultOperandSize == OperandSize.S32 || DefaultOperandSize == OperandSize.S64)
            {
                WriteInt32(value, annotation);
                return;
            }

            throw new NotSupportedException();
        }

        public void WriteUInt16(ushort value, object? annotation = null)
        {
            if (annotation != null)
                _m.AddAnnotation(annotation);
            _m.WriteUInt16(value);
        }

        public void WriteUInt16(uint value, object? annotation = null)
        {
            if (value > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (annotation != null)
                _m.AddAnnotation(annotation);
            _m.WriteUInt16((ushort)value);
        }

        internal void WriteInt16(short value, object? annotation = null)
        {
            if (annotation != null)
                _m.AddAnnotation(annotation);
            _m.WriteInt16(value);
        }

        public byte[] ToArray()
        {
            return _m.ToArray();
        }

        public void WriteAddressSizePrefixIfRequired(Operand rm1, Operand? rm2 = null)
        {
            Memory? m = null;
            if (rm1 is Memory && rm2 is Memory)
                throw new Exception("More than on dereference operand found.");

            if (rm1 is Memory m1) m = m1;
            if (rm2 is Memory m2) m = m2;

            if (m == null)
                return;

            if (m.AddressSize != null && m.AddressSize != DefaultOperandSize)
                WriteByte(0x67);
        }

        public bool Encode(RegisterOrMemory rm, byte registerOrOpcodeExtension)
        {
            if (rm is Memory deref)
            {
                if (deref.AddressSize == OperandSize.S16)
                    return Encode16(deref, registerOrOpcodeExtension);

                if (deref.AddressSize == OperandSize.S32 || deref.AddressSize == OperandSize.S64 || deref.AddressSize == null)
                    return Encode32(deref, registerOrOpcodeExtension);

                return false;
            }

            if (rm is Register r)
            {
                WriteByte(ModRm(0b11, registerOrOpcodeExtension, r.Index));
                return true;
            }

            throw new NotSupportedException();
        }

        private bool Encode16(Memory m, byte registerOrOpcodeExtension)
        {
            if (m.Scale != Scale.S1)
                throw new AssemblerException("16 bit address mode does not allow for a scale != 1");

            if (m.Base == null && m.Index != null)
                m = new Memory(m.Size, m.Index, m.Displacement);

            var disp = (short)m.Displacement;
            var mod = (byte)CalcDispSize(disp);
            if (m.Base?.Index == 5 && m.Index == null && disp == 0)
                mod = 0b01;

            var bi = m.Base?.Index ?? -1;
            var ii = m.Index?.Index ?? -1;

            bool Is(byte r1, byte r2) => (bi == r1 && ii == r2) || (bi == r2 && ii == r1);

            byte rm;
            if (Is(3, 6)) rm = 0b000;
            else if (Is(3, 7)) rm = 0b001;
            else if (Is(5, 6)) rm = 0b010;
            else if (Is(5, 7)) rm = 0b011;
            else if (bi == 6 && ii == -1) rm = 0b100;
            else if (bi == 7 && ii == -1) rm = 0b101;
            else if (bi == 5 && ii == -1) rm = 0b110;
            else if (bi == 3 && ii == -1) rm = 0b111;
            else
            {
                ReportError($"Invalid combination: {m.Base?.Name} + {m.Index?.Name}");
                return false;
            }

            WriteByte(ModRm(mod, registerOrOpcodeExtension, rm));

            if (mod == 0b01)
            {
                AddAnnotation(m.Annotation);
                WriteByte((byte)m.Displacement);
            }

            if (mod == 0b10)
            {
                AddAnnotation(m.Annotation);
                WriteUInt16((ushort)m.Displacement);
            }

            return true;
        }

        private void AddAnnotation(object? annotation)
        {
            if (annotation != null)
                _m.AddAnnotation(annotation);
        }

        private bool Encode32(Memory m, byte registerOrOpcodeExtension)
        {
            //Convert [index_register * 1 + disp] to [base_register + disp]
            if (m.Base == null &&
                m.Index != null &&
                m.Scale == Scale.S1)
                m = new Memory(m.Size, m.Index, m.Displacement);

            //Convert [index_register * 2 + disp] to [base_register + index_register + disp]
            if (m.Base == null &&
                m.Index != null &&
                m.Scale == Scale.S2)
                m = new Memory(m.Size, m.Index, m.Index, Scale.S1, m.Displacement);

            //Since index_register = 4 is not allowed, we check if we can move it to the base register (possible if scale == 1 and base == null)
            if (m.Base == null &&
                m.Index?.Index == 4 &&
                m.Scale == Scale.S1)
                m = new Memory(m.Size, m.Index, m.Displacement);

            //If index_register = 4 and scale = 1, we can exchange index with base register
            if (m.Base != null &&
                m.Index?.Index == 4 &&
                m.Scale == Scale.S1)
                m = new Memory(m.Size, m.Index, m.Base, Scale.S1, m.Displacement);

            //Index register id 4 is not representable
            if (m.Index?.Index == 4)
            {
                ReportError("ESP index is not encodable");
                return false;
            }

            //[address]
            if (m.Base == null && m.Index == null)
            {
                if (DefaultOperandSize == OperandSize.S16)
                {
                    WriteByte(ModRm(0b00, registerOrOpcodeExtension, 6));
                    AddAnnotation(m.Annotation);
                    WriteInt16((short)m.Displacement);
                    return true;
                }
                if (DefaultOperandSize == OperandSize.S32)
                {
                    WriteByte(ModRm(0b00, registerOrOpcodeExtension, 5));
                    AddAnnotation(m.Annotation);
                    WriteInt32(m.Displacement);
                    return true;
                }
                if (DefaultOperandSize == OperandSize.S64)
                {
                    WriteByte(ModRm(0b00, registerOrOpcodeExtension, 4));
                    WriteByte(Sib(0b00, 4, 5));
                    AddAnnotation(m.Annotation);
                    WriteInt32(m.Displacement);
                    return true;
                }
                throw new NotSupportedException();
            }

            var dispSize = CalcDispSize(m.Displacement);

            //Mod describes the amount of bytes used for the displacement (0 = 0, 1 = 8, 2 = 32).
            //If the displacement fits in 8 bits, we use 8 bits. Otherwise we use 32 bits.
            //A special case is if the base register index is 5. In that case we need to always use a displacement (even if it is 0)
            var mod = m.Base?.Index == 5 && m.Displacement == 0 ? (byte)0b01 : (byte)dispSize;

            //[base_register + disp]
            if (m.Base != null && m.Index == null)
            {
                WriteByte(ModRm(mod, registerOrOpcodeExtension, m.Base.Index));
                if (m.Base.Index == 4)
                    WriteByte(Sib((byte)m.Scale, m.Base.Index, m.Base.Index));
                if (mod == 0b01) WriteByte((byte)m.Displacement);
                if (mod == 0b10) WriteUInt32((uint)m.Displacement);
                return true;
            }

            //[index_register * scale + disp]
            if (m.Base == null && m.Index != null)
            {
                WriteByte(ModRm(0b00, registerOrOpcodeExtension, 4));
                WriteByte(Sib((byte)m.Scale, m.Index.Index, 5));
                WriteInt32(m.Displacement);
                return true;
            }

            //[base_register + index_register * scale + disp]
            if (m.Base != null && m.Index != null)
            {
                WriteByte(ModRm(mod, registerOrOpcodeExtension, 4));
                WriteByte(Sib((byte)m.Scale, m.Index.Index, m.Base.Index));
                if (mod == 0b01) WriteByte((byte)m.Displacement);
                if (mod == 0b10) WriteUInt32((uint)m.Displacement);
                return true;
            }

            throw new NotSupportedException();
        }

        private static byte ModRm(byte mod, byte opCodeOrRegister, byte rm) => (byte)((mod << 6) | (opCodeOrRegister << 3) | rm);
        private static byte Sib(byte scale, byte indexIndex, byte @base) => (byte)((scale << 6) | (indexIndex << 3) | @base);

        private static DispSize CalcDispSize(int disp)
        {
            return disp switch
            {
                0 => DispSize.S0,
                >= -128 and <= 127 => DispSize.S8,
                _ => DispSize.S32,
            };
        }

        private enum DispSize
        {
            S0,
            S8,
            S32
        }
    }
}
