using Lucy.Core.Model.Syntax;

namespace Lucy.Core.Parser.Nodes.Trivia
{
    internal class MultiLineCommentTriviaNode : TriviaNode
    {
        public MultiLineCommentTriviaNode(TokenNode start, TokenNode content, TokenNode? end)
        {
            Start = start;
            Content = content;
            End = end;
        }

        public TokenNode Start { get; }
        public TokenNode Content { get; }
        public TokenNode? End { get; }

        public static MultiLineCommentTriviaNode? Read(Code code)
        {
            if (code.Peek() != '/' || code.Peek(1) != '*')
                return null;

            var start = new TokenNode(code.Read(2));

            var len = 0;
            bool foundEnd = false;
            while (true)
            {
                if (code.IsDone)
                    break;
                
                if (code.Peek(len) != '*' || code.Peek(len + 1) != '/')
                {
                    foundEnd = true;
                    break;
                }

                len++;
            }

            var content = new TokenNode(code.Read(len));
            var end = foundEnd ? new TokenNode(code.Read(2)) : null;

            return new MultiLineCommentTriviaNode(start, content, end);
        }
    }
}
