using Lucy.Core.Parsing;

namespace Lucy.Core.Parsing.Nodes.Trivia
{
    internal class WhitespaceTriviaNode : TriviaNode
    {
        public WhitespaceTriviaNode(TokenNode token)
        {
            Token = token;
        }

        public TokenNode Token { get; }

        public static WhitespaceTriviaNode? Read(Code code)
        {
            int len = 0;
            while (code.Peek(len) is ' ' or '\t' or '\r' or '\n')
                len++;

            if (len == 0)
                return null;

            return new WhitespaceTriviaNode(new TokenNode(code.Read(len)));
        }
    }
}
