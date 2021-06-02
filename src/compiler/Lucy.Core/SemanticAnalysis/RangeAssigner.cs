using Lucy.Core.Helper;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes;

namespace Lucy.Core.SemanticAnalysis
{
    internal class RangeAssigner
    {
        internal static void Run(DocumentSyntaxNode node)
        {
            Calc(node, new Position(0, 0, 0));
        }

        private static Position Calc(SyntaxTreeNode node, Position pos)
        {
            var start = pos;

            if (node is TokenNode tokenNode)
            {
                pos = pos.Append(tokenNode.Text);
            }
            else
            {
                foreach (var child in node.GetChildNodes())
                    pos = Calc(child, pos);
            }
            
            var end = pos;
            node.SetAnnotation(new Range(start, end));
            return end;
        }

        public record Range(Position Start, Position End)
        {
            public override string ToString() => $"{Start.Index} - {End.Index}";
        }

        public record Position(int Index, int Line, int Column)
        {
            public Position Append(string str)
            {
                var character = Index + str.Length;
                var line = Line;
                var column = Column;

                for (int i=0;i<str.Length;i++)
                {
                    if (str[i] == '\n')
                    {
                        line++;
                        column = 0;
                    }
                    else
                    {
                        column++;
                    }
                }

                return new Position(character, line, column);
            }
        }
    }
}
