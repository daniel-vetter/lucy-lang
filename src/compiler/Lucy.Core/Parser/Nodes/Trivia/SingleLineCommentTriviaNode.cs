using Lucy.Core.Model.Syntax;

namespace Lucy.Core.Parser.Nodes.Trivia
{
    internal class SingleLineCommentTriviaNode : TriviaNode
    {
        public SingleLineCommentTriviaNode(string comment)
        {
            Comment = comment;
        }

        public string Comment { get; private set; }

        public static SingleLineCommentTriviaNode? Read(Code code)
        {
            if (code.Peek() != '/' || code.Peek(1) != '/')
                return null;

            var len = 2;
            while (true)
            {
                if (code.IsDone || code.Peek(len) == '\n' || code.Peek(len) == '\r' && code.Peek(len + 1) != '\n')
                    break;
                len++;
            }

            return new SingleLineCommentTriviaNode(code.Read(len));
        }
    }
}
