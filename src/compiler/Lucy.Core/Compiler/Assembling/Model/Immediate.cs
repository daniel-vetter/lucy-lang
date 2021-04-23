using System.Text;

namespace Disassembler.Assembling
{
    public record Immediate(OperandSize Size, uint Value) : Operand
    {
        public Immediate(OperandSize Size, int Value, object? annotation) : this(Size, (uint)Value)
        {
            Annotation = annotation;
        }

        public object? Annotation { get; }

        public string ToString(bool outputType, bool outputSign, bool signed)
        {
            var sb = new StringBuilder();

            if (Value == 0 && Annotation != null)
            {
                sb.Append(Annotation);
            }
            else
            {
                if (outputType)
                {
                    sb.Append(Size.ToString());
                    sb.Append(" ");
                }

                if (Size == OperandSize.S8)
                {
                    if (outputSign) sb.Append(signed ? (Value >= 128 ? "-" : "+") : "+");
                    sb.Append(signed && Value >= 128 ? $"0x{256 - Value:x}" : $"0x{Value:x}");
                }
                if (Size == OperandSize.S16)
                {
                    if (outputSign) sb.Append(signed ? (Value >= 32768 ? "-" : "+") : "+");
                    sb.Append(signed && Value >= 32768 ? $"0x{65536 - Value:x}" : $"0x{Value:x}");
                }
                if (Size == OperandSize.S32)
                {
                    if (outputSign) sb.Append(signed ? (Value >= 2147483648 ? "-" : "+") : "+");
                    sb.Append(signed && Value >= 2147483648 ? $"0x{4294967296 - Value:x}" : $"0x{Value:x}");
                }
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString(true, false, false);
        }
    }
}
