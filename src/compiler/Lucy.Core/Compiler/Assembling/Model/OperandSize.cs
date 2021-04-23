using System;

namespace Disassembler.Assembling
{
    public record OperandSize(ushort Bits, string Name)
    {
        public static readonly OperandSize S8 = new OperandSize(8, "byte");
        public static readonly OperandSize S16 = new OperandSize(16, "word");
        public static readonly OperandSize S32 = new OperandSize(32, "dword");
        public static readonly OperandSize S64 = new OperandSize(64, "qword");

        public static OperandSize FromBits(ushort bits)
        {
            return bits switch
            {
                8 => S8,
                16 => S16,
                32 => S32,
                64 => S64,
                _ => throw new NotSupportedException()
            };
        }

        public override string ToString() => Name;
    }
}
