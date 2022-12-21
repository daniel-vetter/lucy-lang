using Lucy.Core.Parsing.Nodes.Expressions;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements;

public static class ExpressionStatementSyntaxNodeParser
{
    public static ExpressionStatementSyntaxNode? Read(Reader reader)
    {
        return reader.WithCache(nameof(ExpressionStatementSyntaxNodeParser), static r =>
        {
            if (ExpressionSyntaxNodeParser.TryRead(r, out var result))
                return ExpressionStatementSyntaxNode.Create(result);
            return null;
        });

    }
}