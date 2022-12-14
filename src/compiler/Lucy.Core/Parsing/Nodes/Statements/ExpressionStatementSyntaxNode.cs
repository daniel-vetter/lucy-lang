using Lucy.Core.Parsing.Nodes.Expressions;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements;

public static class ExpressionStatementSyntaxNodeParser
{
    public static ExpressionStatementSyntaxNodeBuilder? Read(Reader reader)
    {
        return reader.WithCache(nameof(ExpressionStatementSyntaxNodeParser), static code =>
        {
            if (ExpressionSyntaxNodeParser.TryRead(code, out var result))
                return new ExpressionStatementSyntaxNodeBuilder(result);
            return null;
        });

    }
}