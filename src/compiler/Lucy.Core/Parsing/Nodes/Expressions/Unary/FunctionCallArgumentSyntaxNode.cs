using Lucy.Core.Model;
using System.Collections.Immutable;
using Lucy.Core.Parsing.Nodes.Stuff;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public static class FunctionCallArgumentSyntaxNodeParser
{
    private const string _listCacheKey = "List" + nameof(FunctionCallArgumentSyntaxNodeParser);

    public static ImmutableArray<FunctionCallArgumentSyntaxNode> Read(Reader reader)
    {
        return reader.WithCache(_listCacheKey, static (r, _) =>
        {
            var result = ImmutableArray.CreateBuilder<FunctionCallArgumentSyntaxNode>();
            while (true)
            {
                var next = r.WithCache(nameof(FunctionCallArgumentSyntaxNodeParser), static (r, _) =>
                {
                    if (!ExpressionSyntaxNodeParser.TryRead(r, out var expression))
                        return null;

                    var separator = TokenNodeParser.TryReadExact(r, ",");

                    return FunctionCallArgumentSyntaxNode.Create(expression, separator);
                });

                if (next == null)
                    break;

                result.Add(next);

                if (next.Seperator == null)
                    break;
            }

            return result.ToImmutable();
        });
    }
}