using System.Collections.Generic;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Trivia
{
    public abstract class TriviaNodeParser : SyntaxTreeNode
    {
        public static List<TriviaNode> ReadList(Code code)
        {
            var l = new List<TriviaNode>();

            while (!code.IsDone)
            {
                var next =
                   WhitespaceTriviaNodeParser.Read(code) ??
                   SingleLineCommentTriviaNodeParser.Read(code) ??
                   (TriviaNode?)MultiLineCommentTriviaNodeParser.Read(code);

                if (next == null)
                    break;

                l.Add(next);
            }
            
            return l;
        }
    }
}
