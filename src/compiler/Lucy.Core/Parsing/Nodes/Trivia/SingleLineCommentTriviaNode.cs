using Lucy.Core.Parsing;

namespace Lucy.Core.Parsing.Nodes.Trivia
{
    internal class SingleLineCommentTriviaNode : TriviaNode
    {
        public SingleLineCommentTriviaNode(TokenNode start, TokenNode content)
        {
            Start = start;
            Content = content;
        }

        public TokenNode Start { get; }
        public TokenNode Content { get; }

        public static SingleLineCommentTriviaNode? Read(Code code)
        {
            if (code.Peek() != '/' || code.Peek(1) != '/')
                return null;
            var start = new TokenNode(code.Read(2));
           
            int len = 0;
            while (!code.IsDone)
            {
                if (code.Peek(len) == '\n')
                {
                    len++;
                    break;
                }
                else
                    len++;
            }

            return new SingleLineCommentTriviaNode(start, new TokenNode(code.Read(len)));
        }
    }
}
