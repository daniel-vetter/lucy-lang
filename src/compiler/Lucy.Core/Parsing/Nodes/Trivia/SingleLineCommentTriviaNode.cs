using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Trivia
{
    internal class SingleLineCommentTriviaNodeParser
    {
        public static SingleLineCommentTriviaNodeBuilder? Read(Code code)
        {
            if (code.Peek() != '/' || code.Peek(1) != '/')
                return null;
            var start = new TokenNodeBuilder(code.Read(2));
           
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

            return new SingleLineCommentTriviaNodeBuilder(start, new TokenNodeBuilder(code.Read(len)));
        }
    }
}
