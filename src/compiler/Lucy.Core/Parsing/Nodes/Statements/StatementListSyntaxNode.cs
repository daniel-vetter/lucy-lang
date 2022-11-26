using Lucy.Core.Parsing.Nodes.Token;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements;

public class StatementListSyntaxNodeParser
{
    public static StatementListSyntaxNodeBuilder ReadStatementsWithoutBlock(Code code)
    {
        var result = new List<StatementSyntaxNodeBuilder>();
        while (StatementSyntaxNodeParser.TryRead(code, out var statement))
        {
            result.Add(statement);
        }

        return new StatementListSyntaxNodeBuilder(null, result, null);
    }

    public static bool TryReadStatementBlock(Code code, [NotNullWhen(true)] out StatementListSyntaxNodeBuilder? result)
    {
        if (!SyntaxElementParser.TryReadExact(code, "{", out var blockStart))
        {
            result = null;
            return false;
        }

        var list = new List<StatementSyntaxNodeBuilder>();
        while (StatementSyntaxNodeParser.TryRead(code, out var statement))
        {
            list.Add(statement);
        }

        if (!SyntaxElementParser.TryReadExact(code, "}", out var blockEnd))
            blockEnd = SyntaxElementParser.Missing("Code block end '}' expected.");

        result = new StatementListSyntaxNodeBuilder(blockStart, list, blockEnd);
        return true;
    }
}