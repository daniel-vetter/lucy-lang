using Disassembler.Assembling.Infrastructure;
using Lucy.Core.Compiler.Assembling.Infrastructure;

namespace Disassembler.Assembling.Model
{
    public abstract record AssemblerStatement(string? Comment = null)
    {
        public abstract void Write(AssemblyWriter w);
    }

    public record EmptyLine(string? Comment) : AssemblerStatement(Comment)
    {
        public override void Write(AssemblyWriter w)
        {
            w.WriteComment(Comment);
            w.WriteNewLine();
        }
    }

    public record Label(object Key, string? Comment = null) : AssemblerStatement(Comment)
    {
        public override void Write(AssemblyWriter w)
        {
            w.Write(Key.ToString() ?? "");
            w.Write(":");
            w.WriteComment(Comment);
            w.WriteNewLine();
        }
    }

    public abstract record Operation : AssemblerStatement
    {
        public abstract void Write(MachineCodeWriter w);
    }
}
