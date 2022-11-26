using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements;

public class StatementSyntaxNodeParser
{
    public static bool TryRead(Code code, [NotNullWhen(true)] out StatementSyntaxNodeBuilder? result)
    {
        result =
            ImportStatementParser.Read(code) ??
            FunctionDeclarationStatementSyntaxNodeParser.Read(code) ??
            (StatementSyntaxNodeBuilder?)ExpressionStatementSyntaxNodeParser.Read(code);

        return result != null;
    }
}