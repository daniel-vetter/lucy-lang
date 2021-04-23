using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Disassembler.Assembling
{
    public interface Operand
    {
        OperandSize Size { get; }
    };
    public interface RegisterOrMemory : Operand { };

    public enum Scale
    {
        S1 = 0,
        S2 = 1,
        S4 = 2,
        S8 = 3
    }

    public class Memory : RegisterOrMemory
    {
        public Memory(OperandSize size, uint address, object? annotation = null)
        {
            Size = size;
            Annotation = annotation;
            Displacement = (int)address;
        }

        public Memory(OperandSize size, Register baseRegister)
        {
            Size = size;
            Base = baseRegister;
        }

        public Memory(OperandSize size, Register baseRegister, int displacement)
        {
            Size = size;
            Base = baseRegister;
            Displacement = displacement;
        }

        public Memory(OperandSize size, Register indexRegister, Scale scale = Scale.S1, int displacement = 0)
        {
            Size = size;
            Index = indexRegister;
            Scale = scale;
            Displacement = displacement;
        }

        public Memory(OperandSize size, Register baseRegister, Register indexRegister, Scale scale = Scale.S1, int displacement = 0)
        {
            Size = size;
            Base = baseRegister;
            Index = indexRegister;
            Scale = scale;
            Displacement = displacement;

            if (indexRegister == null && scale != Scale.S1) throw new NotSupportedException();
        }

        public Register? Base { get; }
        public Register? Index { get; }
        public Scale Scale { get; }
        public int Displacement { get; }
        public OperandSize Size { get; }
        public object? Annotation { get; }

        public OperandSize? AddressSize
        {
            get
            {
                var baseSize = Base?.Size;
                var indexSize = Index?.Size;

                if (baseSize != null && indexSize != null && baseSize != indexSize)
                    throw new AssemblerException("Base size did not match index size");

                return baseSize ?? indexSize;
            }
        }

        public bool IsAbsolute => Base == null && Index == null;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Size);

            if (Base == null && Index == null)
            {
                sb.Append(" [");
                if (Displacement == 0 && Annotation != null)
                {
                    sb.Append(Annotation);
                }
                else
                {
                    sb.Append("0x");
                    sb.Append(Displacement.ToString("X2"));
                    sb.Append(" ");
                    sb.Append(Annotation);
                }
                sb.Append("]");
            }
            else
            {
                sb.Append(" [");
                if (Base != null)
                {
                    sb.Append(Base);
                }
                if (Index != null)
                {
                    if (Base != null)
                    {
                        sb.Append(" + ");
                    }
                    sb.Append(Index);
                    sb.Append(" * ");
                    sb.Append(Scale switch { Scale.S1 => 1, Scale.S2 => 2, Scale.S4 => 4, Scale.S8 => 8, _ => throw new NotSupportedException(Scale.ToString()) });
                }
                if (Displacement > 0)
                {
                    sb.Append(" + ");
                    sb.Append(Displacement);
                }
                if (Displacement < 0)
                {
                    sb.Append(" - ");
                    sb.Append(-Displacement);
                }
                sb.Append("]");
            }
            return sb.ToString();
        }
    }

    public static class OperandEx
    {
        public static bool IsImmediate8(this Operand operand, [NotNullWhen(true)] out Immediate? imm8)
        {
            if (operand is Immediate casted && casted.Size == OperandSize.S8)
            {
                imm8 = casted;
                return true;
            }
            imm8 = default;
            return false;
        }

        public static bool IsImmediate16(this Operand operand, [NotNullWhen(true)] out Immediate? imm16)
        {
            if (operand is Immediate casted && casted.Size == OperandSize.S16)
            {
                imm16 = casted;
                return true;
            }
            imm16 = default;
            return false;
        }

        public static bool IsImmediate32(this Operand operand, [NotNullWhen(true)] out Immediate? imm32)
        {
            if (operand is Immediate casted && casted.Size == OperandSize.S32)
            {
                imm32 = casted;
                return true;
            }
            imm32 = default;
            return false;
        }

        public static bool IsImmediate64(this Operand operand, [NotNullWhen(true)] out Immediate? imm64)
        {
            if (operand is Immediate casted && casted.Size == OperandSize.S64)
            {
                imm64 = casted;
                return true;
            }
            imm64 = default;
            return false;
        }

        public static bool IsRegisterOrDereference8(this Operand operand, [NotNullWhen(true)] out RegisterOrMemory? rm8)
        {
            if (operand is RegisterOrMemory casted && casted.Size == OperandSize.S8)
            {
                rm8 = casted;
                return true;
            }
            rm8 = default;
            return false;
        }

        public static bool IsRegisterOrDereference16(this Operand operand, [NotNullWhen(true)] out RegisterOrMemory? rm16)
        {
            if (operand is RegisterOrMemory casted && casted.Size == OperandSize.S16)
            {
                rm16 = casted;
                return true;
            }
            rm16 = default;
            return false;
        }

        public static bool IsRegisterOrDereference32(this Operand operand, [NotNullWhen(true)] out RegisterOrMemory? rm32)
        {
            if (operand is RegisterOrMemory casted && casted.Size == OperandSize.S32)
            {
                rm32 = casted;
                return true;
            }
            rm32 = default;
            return false;
        }

        public static bool IsRegisterOrDereference64(this Operand operand, [NotNullWhen(true)] out RegisterOrMemory? rm64)
        {
            if (operand is RegisterOrMemory casted && casted.Size == OperandSize.S64)
            {
                rm64 = casted;
                return true;
            }
            rm64 = default;
            return false;
        }

        public static bool IsDereference8(this Operand operand, [NotNullWhen(true)] out Memory? m8, bool? isAbsolute = null)
        {
            if (operand is Memory casted && casted.Size == OperandSize.S8 && (!isAbsolute.HasValue || isAbsolute.Value == casted.IsAbsolute))
            {
                m8 = casted;
                return true;
            }
            m8 = default;
            return false;
        }

        public static bool IsDereference16(this Operand operand, [NotNullWhen(true)] out Memory? m16, bool? isAbsolute = null)
        {
            if (operand is Memory casted && casted.Size == OperandSize.S16 && (!isAbsolute.HasValue || isAbsolute.Value == casted.IsAbsolute))
            {
                m16 = casted;
                return true;
            }
            m16 = default;
            return false;
        }

        public static bool IsDereference32(this Operand operand, [NotNullWhen(true)] out Memory? m32, bool? isAbsolute = null)
        {
            if (operand is Memory casted && casted.Size == OperandSize.S32 && (!isAbsolute.HasValue || isAbsolute.Value == casted.IsAbsolute))
            {
                m32 = casted;
                return true;
            }
            m32 = default;
            return false;
        }

        public static bool IsDereference64(this Operand operand, [NotNullWhen(true)] out Memory? m64, bool? isAbsolute = null)
        {
            if (operand is Memory casted && casted.Size == OperandSize.S64 && (!isAbsolute.HasValue || isAbsolute.Value == casted.IsAbsolute))
            {
                m64 = casted;
                return true;
            }
            m64 = default;
            return false;
        }

        public static bool IsRegister8(this Operand operand, [NotNullWhen(true)] out Register? r8, int? index = null)
        {
            if (operand is Register casted && casted.Size == OperandSize.S8 && (!index.HasValue || index.Value == casted.Index))
            {
                r8 = casted;
                return true;
            }
            r8 = default;
            return false;
        }

        public static bool IsRegister16(this Operand operand, [NotNullWhen(true)] out Register? r16, int? index = null)
        {
            if (operand is Register casted && casted.Size == OperandSize.S16 && (!index.HasValue || index.Value == casted.Index))
            {
                r16 = casted;
                return true;
            }
            r16 = default;
            return false;
        }

        public static bool IsRegister32(this Operand operand, [NotNullWhen(true)] out Register? r32, int? index = null)
        {
            if (operand is Register casted && casted.Size == OperandSize.S32 && (!index.HasValue || index.Value == casted.Index))
            {
                r32 = casted;
                return true;
            }
            r32 = default;
            return false;
        }

        public static bool IsRegister64(this Operand operand, [NotNullWhen(true)] out Register? r64, int? index = null)
        {
            if (operand is Register casted && casted.Size == OperandSize.S64 && (!index.HasValue || index.Value == casted.Index))
            {
                r64 = casted;
                return true;
            }
            r64 = default;
            return false;
        }
    }
}
