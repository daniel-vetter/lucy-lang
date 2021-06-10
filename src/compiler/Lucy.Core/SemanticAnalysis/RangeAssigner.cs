using Lucy.Core.Helper;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes;
using System;

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
            if (node.Source is SourceCode sourceCodeSource)
            {
                sourceCodeSource.Range = new Model.Syntax.Range(start, end);
            }
            if (node.Source is Syntetic synteticSource)
            {
                synteticSource.Position = start;
            }
            return end;
        }
    }

    public static class SyntaxNodeExtension
    {
        public static Model.Syntax.Range GetRange(this SyntaxTreeNode node)
        {
            if (node.Source is SourceCode sourceCodeSource)
                return sourceCodeSource.Range ?? throw new Exception("Node has not valid range set.");

            if (node.Source is Syntetic synteticSource)
            {
                if (synteticSource.Position == null)
                    throw new Exception("Node has not valid range set.");

                return new Model.Syntax.Range(synteticSource.Position, synteticSource.Position);
            }

            throw new Exception("No range avalilible");
        }
    }
}
