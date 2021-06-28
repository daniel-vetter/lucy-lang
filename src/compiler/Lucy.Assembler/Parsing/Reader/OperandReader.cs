using System;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Assembler.Parsing.Reader
{
    public static class OperandReader
    {
        public static IOperand ReadOperand(this AsmReader reader)
        {
            if (reader.TryReadOperand(out var operand))
                return operand;
            throw new Exception("Could not read an operand");
        }

        public static bool TryReadOperand(this AsmReader reader, [NotNullWhen(true)] out IOperand? operand)
        {
            if (reader.TryReadMemory(out var memory))
            {
                operand = memory;
                return true;
            }

            if (reader.TryReadRegister(out var register))
            {
                operand = register;
                return true;
            }

            if (reader.TryReadImmediate(out var immediate))
            {
                operand = immediate;
                return true;
            }

            if (reader.TryReadLabelRef(out var labelRef))
            {
                operand = labelRef;
                return true;
            }

            operand = null;
            return false;
        }

        public static bool TryReadTwoOperands(this AsmReader reader, [NotNullWhen(true)] out IOperand? operand1, [NotNullWhen(true)] out IOperand? operand2)
        {
            using var t = reader.BeginTransaction();

            if (!reader.TryReadOperand(out var o1))
            {
                operand1 = null;
                operand2 = null;
                return false;
            }

            reader.TryReadWhitespace();
            if (reader.Read() != ',')
            {
                operand1 = null;
                operand2 = null;
                return false;
            }

            if (!reader.TryReadOperand(out var o2))
            {
                operand1 = null;
                operand2 = null;
                return false;
            }

            t.Commit();
            operand1 = o1;
            operand2 = o2;
            return true;
        }

        public static bool TryReadOperandSize(this AsmReader reader, [NotNullWhen(true)] out OperandSize? size)
        {
            using var t = reader.BeginTransaction();

            if (reader.TryReadKeyword("byte"))
            {
                t.Commit();
                size = OperandSize.S8;
                return true;
            }

            if (reader.TryReadKeyword("word"))
            {
                t.Commit();
                size = OperandSize.S16;
                return true;
            }

            if (reader.TryReadKeyword("dword"))
            {
                t.Commit();
                size = OperandSize.S32;
                return true;
            }

            if (reader.TryReadKeyword("qword"))
            {
                t.Commit();
                size = OperandSize.S64;
                return true;
            }

            size = default;
            return false;
        }
    }
}
