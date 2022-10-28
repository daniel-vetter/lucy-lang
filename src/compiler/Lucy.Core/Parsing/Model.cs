namespace Lucy.Core.Parsing
{
    public abstract class SyntaxTreeNode
    {
        public NodeId NodeId { get; set; } = NodeId.Uninitalized;
        public SyntaxTreeNodeSource Source { get; set; } = new SourceCode();
    }

    public abstract class SyntaxTreeNodeSource
    {
    }

    public class Syntetic : SyntaxTreeNodeSource
    {
        public Syntetic(string? errorMessage, Position? position)
        {
            ErrorMessage = errorMessage;
            Position = position;
        }

        public string? ErrorMessage { get; }
        public Position? Position { get; set; }
    }

    public class SourceCode : SyntaxTreeNodeSource
    {
        public Range? Range { get; set; }
    }

    public class Generated : SyntaxTreeNodeSource
    {
    }

    public class TokenNode : SyntaxTreeNode
    {
        public TokenNode(string text = "")
        {
            Text = text;
        }

        public static TokenNode Missing(string? errorMessage = null)
        {
            return new TokenNode("") { Source = new Syntetic(errorMessage, null) };
        }

        public string Text { get; }
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