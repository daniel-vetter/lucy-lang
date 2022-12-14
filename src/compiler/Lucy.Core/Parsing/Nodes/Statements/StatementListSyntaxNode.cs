using System.Collections.Generic;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements;

public static class StatementListSyntaxNodeParser
{
    private const string _statementListWithBlockCacheKey = "WithBlock" + nameof(StatementListSyntaxNodeParser);
    private const string _statementListWithoutBlockCacheKey = "WithoutBlock" + nameof(StatementListSyntaxNodeParser);

    public static StatementListSyntaxNodeBuilder ReadStatementsWithoutBlock(Reader reader)
    {
        return reader.WithCache(_statementListWithoutBlockCacheKey, static code =>
        {
            var result = new List<StatementSyntaxNodeBuilder>();
            while (StatementSyntaxNodeParser.TryRead(code, out var statement)) 
                result.Add(statement);
            return new StatementListSyntaxNodeBuilder(null, result, null);
        });
    }

    public static StatementListSyntaxNodeBuilder? TryReadStatementBlock(Reader reader)
    {
        return reader.WithCache(_statementListWithBlockCacheKey, static code =>
        {
            if (!TokenNodeParser.TryReadExact(code, "{", out var blockStart))
                return null;

            var list = new List<StatementSyntaxNodeBuilder>();
            while (StatementSyntaxNodeParser.TryRead(code, out var statement))
                list.Add(statement);

            if (!TokenNodeParser.TryReadExact(code, "}", out var blockEnd))
                blockEnd = TokenNodeParser.Missing("'}' expected");

            return new StatementListSyntaxNodeBuilder(blockStart, list, blockEnd);
        });
    }
}