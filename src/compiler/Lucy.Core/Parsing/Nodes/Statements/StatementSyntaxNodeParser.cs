using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements;

public static class StatementSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out StatementSyntaxNode? result)
    {
        result =
            VariableDeclarationStatementSyntaxNodeParser.Read(reader) ??
            ImportStatementParser.Read(reader) ??
            FunctionDeclarationStatementSyntaxNodeParser.Read(reader) ??
            (StatementSyntaxNode?)ExpressionStatementSyntaxNodeParser.Read(reader);

        return result != null;
    }
}