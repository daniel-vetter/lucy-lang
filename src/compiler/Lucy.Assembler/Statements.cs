using Lucy.Assembler.Infrastructure;

namespace Lucy.Assembler
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

    public record Label(object Key, string? Comment = null) : Operation(Comment)
    {
        public override void Write(AssemblyWriter w)
        {
            w.Write(Key.ToString() ?? "");
            w.Write(":");
            w.WriteComment(Comment);
            w.WriteNewLine();
        }

        public override void Write(MachineCodeWriter w)
        {
            w.WriteAnnotaton(new AsmLabelAnnotation(Key));
        }
    }

    public record AsmLabelAnnotation(object Key);

    public record AsmLabelRequestAnnotation(object Key);

    public abstract record Operation(string? Comment = null) : AssemblerStatement(Comment)
    {
        public abstract void Write(MachineCodeWriter w);
    }
}
