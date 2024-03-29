﻿using System.Collections.Immutable;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Stuff;

namespace Lucy.Core.Parsing.Nodes.Statements;

public static class StatementListSyntaxNodeParser
{
    private const string _statementListWithBlockCacheKey = "WithBlock" + nameof(StatementListSyntaxNodeParser);
    private const string _statementListWithoutBlockCacheKey = "WithoutBlock" + nameof(StatementListSyntaxNodeParser);

    public static StatementListSyntaxNode ReadStatementsWithoutBlock(Reader reader)
    {
        return reader.WithCache(_statementListWithoutBlockCacheKey, static (r, _) =>
        {
            var result = ImmutableArray.CreateBuilder<StatementSyntaxNode>();
            while (StatementSyntaxNodeParser.TryRead(r, out var statement))
                result.Add(statement);
            return StatementListSyntaxNode.Create(null, result.ToImmutable(), null);
        });
    }

    public static StatementListSyntaxNode? TryReadStatementBlock(Reader reader)
    {
        return reader.WithCache(_statementListWithBlockCacheKey, static (r, _) =>
        {
            if (!TokenNodeParser.TryReadExact(r, "{", out var blockStart))
                return null;

            var list = ImmutableArray.CreateBuilder<StatementSyntaxNode>();
            
            TokenNode? blockEnd;
            while (true)
            {
                while (StatementSyntaxNodeParser.TryRead(r, out var statement))
                    list.Add(statement);

                if (TokenNodeParser.TryReadExact(r, "}", out blockEnd))
                    break;
                
                if (UnknownTokenStatementSyntaxNodeParser.TryRead(r, out var something))
                {
                    list.Add(something);
                    continue;
                }

                blockEnd = TokenNodeParser.Missing("'}' expected");
                break;
            }
            
            return StatementListSyntaxNode.Create(blockStart, list.ToImmutable(), blockEnd);
        });
    }
}