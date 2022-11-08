using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements
{
    public class StatementSyntaxNodeParser
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out StatementSyntaxNode? result)
        {
            result =
                FunctionDeclarationStatementSyntaxNodeParser.Read(code) ??
                (StatementSyntaxNode?)ExpressionStatementSyntaxNodeParser.Read(code);

            return result != null;
        }
    }
}
