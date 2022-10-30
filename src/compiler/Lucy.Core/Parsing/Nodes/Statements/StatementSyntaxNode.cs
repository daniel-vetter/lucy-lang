using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Statements
{
    public abstract record StatementSyntaxNode : SyntaxTreeNode
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out StatementSyntaxNode? result)
        {
            result =
                FunctionDeclarationStatementSyntaxNode.Read(code) ??
                (StatementSyntaxNode?)ExpressionStatementSyntaxNode.Read(code);

            return result != null;
        }
    }
}