using System.Collections.Immutable;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;

namespace Lucy.Core.Parsing.Nodes.Statements;

public static class StatementListSyntaxNodeParser
{
    private const string _statementListWithBlockCacheKey = "WithBlock" + nameof(StatementListSyntaxNodeParser);
    private const string _statementListWithoutBlockCacheKey = "WithoutBlock" + nameof(StatementListSyntaxNodeParser);

    public static StatementListSyntaxNode ReadStatementsWithoutBlock(Reader reader)
    {
        return reader.WithCache(_statementListWithoutBlockCacheKey, static r =>
        {
            var result = ImmutableArray.CreateBuilder<StatementSyntaxNode>();
            while (StatementSyntaxNodeParser.TryRead(r, out var statement)) 
                result.Add(statement);
            return StatementListSyntaxNode.Create(null, result.ToImmutable(), null);
        });
    }

    public static StatementListSyntaxNode? TryReadStatementBlock(Reader reader)
    {
        return reader.WithCache(_statementListWithBlockCacheKey, static r =>
        {
            if (!TokenNodeParser.TryReadExact(r, "{", out var blockStart))
                return null;

            var list = ImmutableArray.CreateBuilder<StatementSyntaxNode>();
            while (StatementSyntaxNodeParser.TryRead(r, out var statement))
                list.Add(statement);

            if (!TokenNodeParser.TryReadExact(r, "}", out var blockEnd))
                blockEnd = TokenNodeParser.Missing("'}' expected");

            return StatementListSyntaxNode.Create(blockStart, list.ToImmutable(), blockEnd);
        });
    }
}