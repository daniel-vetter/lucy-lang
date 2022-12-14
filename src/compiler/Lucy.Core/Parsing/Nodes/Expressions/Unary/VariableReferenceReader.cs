using Lucy.Core.Model;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public static class VariableReferenceExpressionSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out VariableReferenceExpressionSyntaxNodeBuilder? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static VariableReferenceExpressionSyntaxNodeBuilder? TryRead(Reader reader)
    {
        return reader.WithCache(nameof(VariableReferenceExpressionSyntaxNodeParser), static code =>
        {
            if (TokenNodeParser.TryReadIdentifier(code, out var token))
                return new VariableReferenceExpressionSyntaxNodeBuilder(token);
            return null;
        });
    }
}