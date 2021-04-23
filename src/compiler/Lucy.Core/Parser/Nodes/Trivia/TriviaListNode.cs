using Lucy.Core.Model.Syntax;
using System.Collections.Generic;

namespace Lucy.Core.Parser.Nodes.Trivia
{
    public class TriviaListNode
    {
        public TriviaListNode(List<TriviaNode> trivia)
        {
            Trivia = trivia;
        }

        public List<TriviaNode> Trivia { get; }

        public static TriviaListNode Read(Code code)
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

            return new TriviaListNode(l);
        }
    }
}
