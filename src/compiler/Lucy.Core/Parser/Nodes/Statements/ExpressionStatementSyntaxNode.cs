using Lucy.Core.Parser.Nodes.Expressions;

namespace Lucy.Core.Parser.Nodes.Statements
{
    public class ExpressionStatementSyntaxNode : StatementSyntaxNode
    {
        public ExpressionStatementSyntaxNode(ExpressionSyntaxNode expression)
        {
            Expression = expression;
        }

        public ExpressionSyntaxNode Expression { get; set; }

        public static ExpressionStatementSyntaxNode? Read(Code code)
        {
            if (ExpressionSyntaxNode.TryRead(code, out var result))
                return new ExpressionStatementSyntaxNode(result);
            return null;
        }
    }
}