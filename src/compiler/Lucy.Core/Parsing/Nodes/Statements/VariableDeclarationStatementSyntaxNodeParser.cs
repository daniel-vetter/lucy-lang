using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions;

namespace Lucy.Core.Parsing.Nodes.Statements
{
    public static class VariableDeclarationStatementSyntaxNodeParser
    {
        public static VariableDeclarationStatementSyntaxNodeBuilder? Read(Reader reader)
        {
            return reader.WithCache(nameof(VariableDeclarationStatementSyntaxNodeParser), static code =>
            {
                if (!TokenNodeParser.TryReadKeyword(code, "var", out var varKeyword))
                    return null;

                if (!TokenNodeParser.TryReadIdentifier(code, out var variableName))
                    variableName = TokenNodeParser.Missing("Variable name expected");

                if (!TokenNodeParser.TryReadExact(code, "=", out var equalSign))
                    equalSign = TokenNodeParser.Missing("'=' expected");

                if (!ExpressionSyntaxNodeParser.TryRead(code, out var expression))
                    expression = ExpressionSyntaxNodeParser.Missing("Expression expected");

                return new VariableDeclarationStatementSyntaxNodeBuilder(varKeyword, variableName, equalSign, expression);
            });
        }
    }
}
