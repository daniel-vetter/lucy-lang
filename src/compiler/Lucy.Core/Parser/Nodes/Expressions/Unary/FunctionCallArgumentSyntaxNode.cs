using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Token;
using System.Collections.Generic;

namespace Lucy.Core.Parser.Nodes.Expressions.Unary
{
    public class FunctionCallArgumentSyntaxNode : SyntaxTreeNode
    {
        public FunctionCallArgumentSyntaxNode(ExpressionSyntaxNode expression, SyntaxElement? seperator)
        {
            Expression = expression;
            Seperator = seperator;
        }

        public ExpressionSyntaxNode Expression { get; }
        public SyntaxElement? Seperator { get; }

        public static List<FunctionCallArgumentSyntaxNode> Read(Code code)
        {
            var result = new List<FunctionCallArgumentSyntaxNode>();
            while (true)
            {
                if (!ExpressionSyntaxNode.TryRead(code, out var expression))
                    break;

                SyntaxElement.TryReadExact(code, ",", out var seperator);

                result.Add(new FunctionCallArgumentSyntaxNode(expression, seperator));

                if (seperator == null)
                    break;
            }

            return result;
        }
    }
}
