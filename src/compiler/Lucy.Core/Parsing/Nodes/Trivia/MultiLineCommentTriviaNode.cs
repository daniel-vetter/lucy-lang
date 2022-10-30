namespace Lucy.Core.Parsing.Nodes.Trivia
{
    public record MultiLineCommentTriviaNode(TokenNode Start, TokenNode Content, TokenNode? End) : TriviaNode
    {
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
