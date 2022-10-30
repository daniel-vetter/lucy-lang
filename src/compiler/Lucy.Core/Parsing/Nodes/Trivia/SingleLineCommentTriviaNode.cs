﻿namespace Lucy.Core.Parsing.Nodes.Trivia
{
    internal record SingleLineCommentTriviaNode(TokenNode Start, TokenNode Content) : TriviaNode
    {
        public static SingleLineCommentTriviaNode? Read(Code code)
        {
            if (code.Peek() != '/' || code.Peek(1) != '/')
                return null;
            var start = new TokenNode(code.Read(2));
           
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

            return new SingleLineCommentTriviaNode(start, new TokenNode(code.Read(len)));
        }
    }
}
