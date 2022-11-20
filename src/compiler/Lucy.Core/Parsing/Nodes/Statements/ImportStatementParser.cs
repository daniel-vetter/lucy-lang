using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Parsing.Nodes.Token;

namespace Lucy.Core.Parsing.Nodes.Statements
{
    internal class ImportStatementParser
    {
        public static ImportStatementSyntaxNodeBuilder? Read(Code code)
        {
            var t = code.BeginTransaction();

            if (!SyntaxElementParser.TryReadKeyword(code, "import", out var importKeyword))
                return null;

            t.Commit();

            if (!StringConstantExpressionSyntaxNodeParser.TryRead(code, out var stringConstant))
                stringConstant = StringConstantExpressionSyntaxNodeParser.Missing("Expected string constant after import statement. Please specifiy a relative or absolute path to a other lucy file.");

            return new ImportStatementSyntaxNodeBuilder(importKeyword, stringConstant);
        }
    }
}
