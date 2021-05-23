using Disassembler.Assembling;
using System;
using System.Text;

namespace Lucy.Core.Compiler.Assembling.Infrastructure
{
    public class AssemblyWriter
    {
        StringBuilder _sb = new StringBuilder();

        public void Write(string text) => _sb.Append(text);

        public void WritePadding() => _sb.Append("    ");

        public void WriteOperation(string name, params Operand[] operands)
        {
            _sb.Append("    ");
            _sb.Append(name);
            _sb.Append(" ");
            for (int i = 0; i < operands.Length; i++)
            {
                _sb.Append(operands[i]);
                if (i != operands.Length - 1)
                    _sb.Append(",");
            }
        }

        public void WriteComment(string? comment)
        {
            if (comment == null)
                return;

            int lineLength = 0;
            for (int i = _sb.Length - 1; i >= 0 && _sb[i] != '\n'; i--)
                lineLength++;

            if (lineLength > 0)
            {
                _sb.Append(' ', Math.Max(1, 50 - lineLength));
            }
                
            
            _sb.Append("; ");
            _sb.Append(comment);
        }

        internal void WriteNewLine()
        {
            _sb.AppendLine();
        }

        public override string ToString() => _sb.ToString();
    }
}
