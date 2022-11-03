namespace Lucy.Core.Parsing
{
    public abstract record SyntaxTreeNode
    {
        public NodeId NodeId { get; set; } = NodeId.Uninitalized;
        public SyntaxTreeNodeSource Source { get; init; } = new SourceCode();
    }

    public interface ICustomIdElementName
    {
        string CustomIdElementName { get; }
    }

    public abstract record SyntaxTreeNodeSource
    {
    }

    public record Syntetic : SyntaxTreeNodeSource
    {
        public Syntetic(string? errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public string? ErrorMessage { get; }
    }

    public record SourceCode : SyntaxTreeNodeSource
    {
    }

    public record Generated : SyntaxTreeNodeSource
    {
    }

    public record TokenNode(string Text) : SyntaxTreeNode
    {
        public static TokenNode Missing(string? errorMessage = null)
        {
            return new TokenNode("") { Source = new Syntetic(errorMessage) };
        }
    }

    public record Range(Position Start, Position End)
    {
        public override string ToString() => $"{Start.Index} - {End.Index}";

        public bool Contains(int index, bool exclusiveEnd = true) => exclusiveEnd ? index >= Start.Index && index < End.Index : index >= Start.Index && index <= End.Index;
        public bool Contains(int line, int column, bool exclusiveEnd = true)
        {
            if (line < Start.Line || line > End.Line)
                return false;

            var isAfterStart = line == Start.Line && column >= Start.Column || line > Start.Line;
            var isBeforeEnd = exclusiveEnd ? line == End.Line && column < End.Column || line < End.Line : line == End.Line && column < End.Column || line <= End.Line;
            return isAfterStart && isBeforeEnd;
        }
    }

    public record Position(int Index, int Line, int Column)
    {
        public Position Append(string str)
        {
            var character = Index + str.Length;
            var line = Line;
            var column = Column;

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\n')
                {
                    line++;
                    column = 0;
                }
                else
                {
                    column++;
                }
            }

            return new Position(character, line, column);
        }
    }
}