using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Trivia;

namespace Lucy.Core.Parsing.Nodes.Statements;

public static class UnknownTokenStatementSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out UnknownTokenStatementSyntaxNode? result)
    {
        result = Read(reader);
        return result != null;
    }

    public static UnknownTokenStatementSyntaxNode? Read(Reader reader)
    {
        return reader.WithCache(nameof(UnknownTokenStatementSyntaxNodeParser), static r =>
        {
            var sb = new StringBuilder();
            var lastCharType = -1;
            while (true)
            {
                var ch = r.Peek();
                if (ch is '\0')
                    break;

                var charType = GetCharType(ch);
                if (charType != lastCharType && lastCharType != -1)
                    break;
                
                sb.Append(ch);
                r.Seek(1);
                lastCharType = charType;
            }

            if (sb.Length == 0)
                return null;

            var str = sb.ToString();
            var syntaxError = ImmutableArray.Create($"Unexpected token \"{str}\"");
            return UnknownTokenStatementSyntaxNode.Create(new TokenNode(null, str, TriviaParser.Read(r), syntaxError));
        });
    }

    private static int GetCharType(char ch)
    {
        return ch switch
        {
            >= 'A' and <= 'Z' or >= 'a' and <= 'z' => 1,
            >= '0' and <= '9' => 2,
            '{' => 3,
            '}' => 4,
            '>' => 5,
            '<' => 6,
            _ => int.MinValue
        };
    }
}