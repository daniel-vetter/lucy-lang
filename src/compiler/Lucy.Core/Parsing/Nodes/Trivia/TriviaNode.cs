using System.Collections.Generic;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Trivia
{
    public abstract class TriviaNodeParser
    {
        public static List<TriviaNodeBuilder> ReadList(Code code)
        {
            var l = new List<TriviaNodeBuilder>();

            while (!code.IsDone)
            {
                var next =
                   WhitespaceTriviaNodeParser.Read(code) ??
                   SingleLineCommentTriviaNodeParser.Read(code) ??
                   (TriviaNodeBuilder?)MultiLineCommentTriviaNodeParser.Read(code);

                if (next == null)
                    break;

                l.Add(next);
            }
            
            return l;
        }
    }
}
