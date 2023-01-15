using System.Collections.Immutable;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Stuff;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

internal static class FunctionDeclarationModifierList
{
    public static ImmutableArray<TokenNode> Read(Reader reader)
    {
        return reader.WithCache(nameof(FunctionDeclarationModifierList), static (r, _) =>
        {
            var l = ImmutableArray.CreateBuilder<TokenNode>();

            while (true)
            {
                if (TokenNodeParser.TryReadKeyword(r, "extern", out var externKeyword))
                {
                    l.Add(externKeyword);
                    continue;
                }

                if (TokenNodeParser.TryReadKeyword(r, "cdecl", out var cdeclKeyword))
                {
                    l.Add(cdeclKeyword);
                    continue;
                }

                if (TokenNodeParser.TryReadKeyword(r, "stdcall", out var stdcallKeyword))
                {
                    l.Add(stdcallKeyword);
                    continue;
                }

                break;
            }

            return l.ToImmutable();
        });
    }
}