using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions;

namespace Lucy.Core.Parsing.Nodes.Statements
{
    public static class VariableDeclarationStatementSyntaxNodeParser
    {
        public static VariableDeclarationStatementSyntaxNodeBuilder? Read(Code code)
        {
            using var t = code.BeginTransaction();

            if (!SyntaxElementParser.TryReadKeyword(code, "var", out var varKeyword))
                return null;

            if (!SyntaxElementParser.TryReadIdentifier(code, out var variableName))
                variableName = SyntaxElementParser.Missing("Variable name expected");

            if (!SyntaxElementParser.TryReadExact(code, "=", out var equalSign))
                equalSign = SyntaxElementParser.Missing("'=' expected");

            if (!ExpressionSyntaxNodeParser.TryRead(code, out var expression))
                expression = ExpressionSyntaxNodeParser.Missing("Expression expected");

            t.Commit();

            return new VariableDeclarationStatementSyntaxNodeBuilder(varKeyword, variableName, equalSign, expression);

        }
    }
}
