using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Statements
{
    public record StatementListSyntaxNode(SyntaxElement? BlockStart, ComparableReadOnlyList<StatementSyntaxNode> Statements, SyntaxElement? BlockEnd) : StatementSyntaxNode
    {
        public static StatementListSyntaxNode ReadStatementsWithoutBlock(Code code)
        {
            var result = new ComparableReadOnlyList<StatementSyntaxNode>.Builder();
            while (TryRead(code, out var statement))
            {
                result.Add(statement);
            }

            return new StatementListSyntaxNode(null, result.Build(), null);
        }

        public static bool TryReadStatementBlock(Code code, [NotNullWhen(true)] out StatementListSyntaxNode? result)
        {
            if (!SyntaxElement.TryReadExact(code, "{", out var blockStart))
            {
                result = null;
                return false;
            }
            
            var list = new ComparableReadOnlyList<StatementSyntaxNode>.Builder();
            while (TryRead(code, out var statement))
            {
                list.Add(statement);
            }

            if (!SyntaxElement.TryReadExact(code, "}", out var blockEnd))
                code.ReportError("Code block end '}' expected.");

            result = new StatementListSyntaxNode(blockStart, list.Build(), blockEnd);
            return true;
        }
    }
}
