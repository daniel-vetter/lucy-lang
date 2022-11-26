using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Trivia;

internal class WhitespaceTriviaNodeParser
{
    public static WhitespaceTriviaNodeBuilder? Read(Code code)
    {
        int len = 0;
        while (code.Peek(len) is ' ' or '\t' or '\r' or '\n')
            len++;

        if (len == 0)
            return null;

        return new WhitespaceTriviaNodeBuilder(new TokenNodeBuilder(code.Read(len)));
    }
}