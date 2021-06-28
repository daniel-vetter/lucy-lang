using System;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Assembler.Parsing.Reader
{
    public static class MemoryReader
    {
        public static bool TryReadMemory(this AsmReader reader, [NotNullWhen(true)] out Memory? operand)
        {
            using var t = reader.BeginTransaction();

            reader.TryReadOperandSize(out var operandSize);
            
            if (!reader.TryReadExact("["))
            {
                operand = null;
                return false;
            }

            if (!reader.TryReadMemoryBody(operandSize ?? reader.DefaultOperandSize, out var memoryBody))
            {
                operand = null;
                return false;
            }

            if (!reader.TryReadExact("]"))
            {
                operand = null;
                return false;
            }

            operand = memoryBody;
            t.Commit();
            return true;
        }

        private static bool TryReadMemoryBody(this AsmReader reader, OperandSize operandSize, [NotNullWhen(true)] out Memory? operand)
        {
            if (reader.TryReadBaseIndexScaleDisplacment(operandSize, out var bisd))
            {
                operand = bisd;
                return true;
            }

            if (reader.TryReadNumber(out var number))
            {
                operand = new Memory(operandSize, number);
                return true;
            }

            if (reader.TryReadLabelRef(operandSize, out var label))
            {
                operand = label;
                return true;
            }

            operand = default;
            return false;
        }

        private static bool TryReadLabelRef(this AsmReader reader, OperandSize operandSize, [NotNullWhen(true)] out Memory? operand)
        {
            using var t = reader.BeginTransaction();

            if (!reader.TryReadIdentifier(out var identifier))
            {
                operand = null;
                return false;
            }

            t.Commit();
            operand = new Memory(operandSize, 0, identifier);
            return true;
        }

        private static bool TryReadBaseIndexScaleDisplacment(this AsmReader reader, OperandSize operandSize, [NotNullWhen(true)] out Memory? operand)
        {
            var t = reader.BeginTransaction();

            reader.TryReadRegister(out var baseRegister);
            reader.TryReadIndexRegister(out var indexRegister);
            reader.TryReadScale(out var scale);
            reader.TryReadDisplacement(out var displacement);

            if (baseRegister != null)
            {
                if (indexRegister != null)
                {
                    if (scale == null && displacement == null)
                    {
                        t.Commit();
                        operand = new Memory(operandSize, baseRegister, indexRegister);
                        return true;
                    }


                    if (scale != null && displacement == null)
                    {
                        t.Commit();
                        operand = new Memory(operandSize, baseRegister, indexRegister, scale.Value);
                        return true;
                    }

                    if (scale == null && displacement != null)
                    {
                        t.Commit();
                        operand = new Memory(operandSize, baseRegister, indexRegister, displacement: (int)displacement.Value);
                        return true;
                    }

                    if (scale != null && displacement != null)
                    {
                        t.Commit();
                        operand = new Memory(operandSize, baseRegister, indexRegister, scale.Value, (int)displacement);
                        return true;
                    }
                }
                else
                {
                    if (scale == null && displacement == null)
                    {
                        t.Commit();
                        operand = new Memory(operandSize, baseRegister);
                        return true;
                    }

                    if (scale != null && displacement == null)
                    {
                        t.Commit();
                        operand = new Memory(operandSize, baseRegister, scale.Value);
                        return true;
                    }

                    if (scale == null && displacement != null)
                    {
                        t.Commit();
                        operand = new Memory(operandSize, baseRegister, displacement: (int)displacement.Value);
                        return true;
                    }

                    if (scale != null && displacement != null)
                    {
                        t.Commit();
                        operand = new Memory(operandSize, baseRegister, scale.Value, (int)displacement.Value);
                        return true;
                    }
                }
            }

            operand = null;
            return false;
        }

        private static bool TryReadIndexRegister(this AsmReader reader, [NotNullWhen(true)] out Register? register)
        {
            using var t = reader.BeginTransaction();

            if (!reader.TryReadSign(out var sign) || sign != Sign.Plus)
            {
                register = null;
                return false;
            }

            if (!reader.TryReadRegister(out var indexRegister))
            {
                register = null;
                return false;
            }

            t.Commit();
            register = indexRegister;
            return true;
        }

        private static bool TryReadScale(this AsmReader reader, [NotNullWhen(true)] out Scale? scale)
        {
            using var t = reader.BeginTransaction();

            if (!reader.TryReadExact("*"))
            {
                scale = default;
                return false;
            }

            if (!reader.TryReadNumber(out var scaleNumber))
            {
                scale = default;
                return false;
            }

            Scale? scaleEnum = scaleNumber switch
            {
                1 => Scale.S1,
                2 => Scale.S2,
                4 => Scale.S4,
                8 => Scale.S8,
                _ => null
            };

            if (!scaleEnum.HasValue)
            {
                scale = default;
                return false;
            }

            scale = scaleEnum.Value;
            t.Commit();
            return true;
        }

        private static bool TryReadDisplacement(this AsmReader reader, [NotNullWhen(true)] out uint? displacement)
        {
            using var t = reader.BeginTransaction();

            if (!reader.TryReadSign(out var sign) || sign != Sign.Plus)
            {
                displacement = null;
                return false;
            }

            if (!reader.TryReadNumber(out var number))
            {
                displacement = null;
                return false;
            }

            t.Commit();
            displacement = number;
            return true;

        }
    }
}
