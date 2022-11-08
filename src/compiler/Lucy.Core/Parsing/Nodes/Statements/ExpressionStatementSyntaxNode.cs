using Lucy.Core.Parsing.Nodes.Expressions;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements
{
    public class ExpressionStatementSyntaxNodeParser
    {
        public static ExpressionStatementSyntaxNode? Read(Code code)
        {
            if (ExpressionSyntaxNodeParser.TryRead(code, out var result))
                return new ExpressionStatementSyntaxNode(result);
            return null;
        }
    }
}