using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Collections.Immutable;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public static class FunctionCallArgumentSyntaxNodeParser
{
    private const string _listCacheKey = "List" + nameof(FunctionCallArgumentSyntaxNodeParser);

    public static ImmutableArray<FunctionCallArgumentSyntaxNode> Read(Reader reader)
    {
        return reader.WithCache(_listCacheKey, static code =>
        {
            var result = ImmutableArray.CreateBuilder<FunctionCallArgumentSyntaxNode>();
            while (true)
            {
                var next = code.WithCache(nameof(FunctionCallArgumentSyntaxNodeParser), static r =>
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