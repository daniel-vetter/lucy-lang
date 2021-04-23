using Lucy.Core.Model.Syntax;

namespace Lucy.Core.Parser.Nodes.Trivia
{
    internal class WhitespaceTriviaNode : TriviaNode
    {
        public WhitespaceTriviaNode(string withespace)
        {
            Withespace = withespace;
        }

        public static WhitespaceTriviaNode? Read(Code code)
        {
            int len = 0;
            while (code.Peek(len) is ' ' or '\t' or '\r' or '\n')
                len++;

            if (len == 0)
                return null;

            return new WhitespaceTriviaNode(code.Read(len));
        }

        public string Withespace { get; }
    }
}
