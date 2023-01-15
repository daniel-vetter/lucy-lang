using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Parsing.Nodes.Stuff;

namespace Lucy.Core.Parsing.Nodes.Statements;

internal static class ImportStatementParser
{
    public static ImportStatementSyntaxNode? Read(Reader reader)
    {
        return reader.WithCache(nameof(ImportStatementParser), static (r, _) =>
        {
            if (!TokenNodeParser.TryReadKeyword(r, "import", out var importKeyword))
                return null;

            if (!StringConstantExpressionSyntaxNodeParser.TryRead(r, out var stringConstant))
                stringConstant = StringConstantExpressionSyntaxNodeParser.Missing(r, "Expected string constant after import statement. Please specify a relative or absolute path to a other lucy file.");

            return ImportStatementSyntaxNode.Create(importKeyword, stringConstant);
        });
    }
}