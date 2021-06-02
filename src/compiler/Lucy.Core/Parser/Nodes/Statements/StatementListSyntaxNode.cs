using Lucy.Core.Parser.Nodes.Token;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Statements
{
    public class StatementListSyntaxNode : StatementSyntaxNode
    {
        public StatementListSyntaxNode(SyntaxElement? blockStart, List<StatementSyntaxNode> statements, SyntaxElement? blockEnd)
        {
            BlockStart = blockStart;
            Statements = statements;
            BlockEnd = blockEnd;
        }

        public SyntaxElement? BlockStart { get; }
        public List<StatementSyntaxNode> Statements { get; }
        public SyntaxElement? BlockEnd { get; }

        public static StatementListSyntaxNode ReadStatementsWithoutBlock(Code code)
        {
            var result = new List<StatementSyntaxNode>();
            while (TryRead(code, out var statement))
            {
                result.Add(statement);
            }

            return new StatementListSyntaxNode(null, result, null);
        }

        public static bool TryReadStatementBlock(Code code, [NotNullWhen(true)] out StatementListSyntaxNode? result)
        {
            if (!SyntaxElement.TryReadExact(code, "{", out var blockStart))
            {
                result = null;
                return false;
            }
            
            var list = new List<StatementSyntaxNode>();
            while (TryRead(code, out var statement))
            {
                list.Add(statement);
            }

            if (!SyntaxElement.TryReadExact(code, "}", out var blockEnd))
                code.ReportError("Code block end '}' expected.");

            result = new StatementListSyntaxNode(blockStart, list, blockEnd);
            return true;
        }
    }
}
