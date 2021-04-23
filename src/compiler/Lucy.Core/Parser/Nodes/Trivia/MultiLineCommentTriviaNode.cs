using Lucy.Core.Model.Syntax;

namespace Lucy.Core.Parser.Nodes.Trivia
{
    internal class MultiLineCommentTriviaNode : TriviaNode
    {
        public MultiLineCommentTriviaNode(string comment)
        {
            Comment = comment;
        }

        public string Comment { get; }

        public static MultiLineCommentTriviaNode? Read(Code code)
        {
            if (code.Peek() != '/' || code.Peek(1) != '*')
                return null;

            var len = 2;
            while (true)
            {
                if (code.Peek(len) == '*' && code.Peek(len + 1) != '/')
                {
                    len += 2;
                    break;
                }

                if (code.IsDone)
                {
                    code.ReportError("Missing '*/'", code.Position + len);
                    break;
                }

                len++;
            }

            return new MultiLineCommentTriviaNode(code.Read(len));
        }
    }
}
