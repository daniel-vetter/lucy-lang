using Lucy.Core.Parsing;
using System.Collections.Generic;

namespace Lucy.Core.Parsing.Nodes.Trivia
{
    public abstract class TriviaNode : SyntaxTreeNode
    {
        public static List<TriviaNode> ReadList(Code code)
        {
            var l = new List<TriviaNode>();

            while (!code.IsDone)
            {
                var next =
                   WhitespaceTriviaNode.Read(code) ??
                   SingleLineCommentTriviaNode.Read(code) ??
                   (TriviaNode?)MultiLineCommentTriviaNode.Read(code);

                if (next == null)
                    break;

                l.Add(next);
            }

            return l;
        }
    }
}
