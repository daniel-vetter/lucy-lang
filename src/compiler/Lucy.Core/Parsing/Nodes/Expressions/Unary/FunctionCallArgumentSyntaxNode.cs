using Lucy.Core.Parsing.Nodes.Token;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary
{
    public record FunctionCallArgumentSyntaxNode(ExpressionSyntaxNode expression, SyntaxElement? seperator) : SyntaxTreeNode
    {
        public static ComparableReadOnlyList<FunctionCallArgumentSyntaxNode> Read(Code code)
        {
            var result = new ComparableReadOnlyList<FunctionCallArgumentSyntaxNode>.Builder();
            while (true)
            {
                if (!ExpressionSyntaxNode.TryRead(code, out var expression))
                    break;

                SyntaxElement.TryReadExact(code, ",", out var seperator);

                result.Add(new FunctionCallArgumentSyntaxNode(expression, seperator));

                if (seperator == null)
                    break;
            }

            return result.Build();
        }
    }
}
