namespace Lucy.Core.Parsing.Nodes.Trivia
{
    public abstract record TriviaNode : SyntaxTreeNode
    {
        public static ComparableReadOnlyList<TriviaNode> ReadList(Code code)
        {
            var l = new ComparableReadOnlyList<TriviaNode>.Builder();

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

            return l.Build();
        }
    }
}
