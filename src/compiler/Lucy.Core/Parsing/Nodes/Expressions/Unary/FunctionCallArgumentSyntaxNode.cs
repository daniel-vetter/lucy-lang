using Lucy.Core.Model;
using System.Collections.Generic;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public static class FunctionCallArgumentSyntaxNodeParser
{
    private const string _listCacheKey = "List" + nameof(FunctionCallArgumentSyntaxNodeParser);

    public static List<FunctionCallArgumentSyntaxNodeBuilder> Read(Reader reader)
    {
        return reader.WithCache(_listCacheKey, static code =>
        {
            var result = new List<FunctionCallArgumentSyntaxNodeBuilder>();
            while (true)
            {
                var next = code.WithCache(nameof(FunctionCallArgumentSyntaxNodeParser), static code =>
                {
                    if (!ExpressionSyntaxNodeParser.TryRead(code, out var expression))
                        return null;

                    var separator = TokenNodeParser.TryReadExact(code, ",");

                    return new FunctionCallArgumentSyntaxNodeBuilder(expression, separator);
                });

                if (next == null)
                    break;

                result.Add(next);

                if (next.Seperator == null)
                    break;
            }

            return result;
        });
    }
}