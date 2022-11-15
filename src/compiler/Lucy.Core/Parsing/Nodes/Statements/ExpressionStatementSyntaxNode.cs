using Lucy.Core.Parsing.Nodes.Expressions;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements
{
    public class ExpressionStatementSyntaxNodeParser
    {
        public static ExpressionStatementSyntaxNodeBuilder? Read(Code code)
        {
            if (ExpressionSyntaxNodeParser.TryRead(code, out var result))
                return new ExpressionStatementSyntaxNodeBuilder(result);
            return null;
        }
    }
}