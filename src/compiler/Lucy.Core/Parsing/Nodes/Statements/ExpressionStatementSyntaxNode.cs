using Lucy.Core.Parsing.Nodes.Expressions;

namespace Lucy.Core.Parsing.Nodes.Statements
{
    public record ExpressionStatementSyntaxNode(ExpressionSyntaxNode expression) : StatementSyntaxNode
    {
        public static ExpressionStatementSyntaxNode? Read(Code code)
        {
            if (ExpressionSyntaxNode.TryRead(code, out var result))
                return new ExpressionStatementSyntaxNode(result);
            return null;
        }
    }
}