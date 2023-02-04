﻿using System.Collections.Immutable;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Stuff;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class FunctionDeclarationParameterSyntaxNodeParser 
{
    private const string _listCacheKey = "List" + nameof(FunctionDeclarationParameterSyntaxNodeParser);

    public static ImmutableArray<FunctionDeclarationParameterSyntaxNode> Read(Reader reader)
    {
        return reader.WithCache(_listCacheKey, static (r, _) =>
        {
            var l = ImmutableArray.CreateBuilder<FunctionDeclarationParameterSyntaxNode>();
            while (true)
            {
                var next = r.WithCache(nameof(FunctionDeclarationParameterSyntaxNodeParser), static (r, _) =>
                {
                    var def = VariableDefinitionSyntaxNodeParser.Read(r);
                    if (def == null)
                        return null;

                    TokenNodeParser.TryReadExact(r, ",", out var separator);

                    return FunctionDeclarationParameterSyntaxNode.Create(def, separator);
                });

                if (next == null)
                    break;
                
                l.Add(next);

                if (next.Seperator == null)
                    break;
            }

            return l.ToImmutable();
        });
    }
}