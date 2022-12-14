using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;

namespace Lucy.Core.Parsing.Nodes.Statements;

internal static class ImportStatementParser
{
    public static ImportStatementSyntaxNodeBuilder? Read(Reader reader)
    {
        return reader.WithCache(nameof(ImportStatementParser), static code =>
        {
            if (!TokenNodeParser.TryReadKeyword(code, "import", out var importKeyword))
                return null;

            if (!StringConstantExpressionSyntaxNodeParser.TryRead(code, out var stringConstant))
                stringConstant = StringConstantExpressionSyntaxNodeParser.Missing("Expected string constant after import statement. Please specifiy a relative or absolute path to a other lucy file.");

            return new ImportStatementSyntaxNodeBuilder(importKeyword, stringConstant);
        });
    }
}