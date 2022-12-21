using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public static class VariableReferenceExpressionSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out VariableReferenceExpressionSyntaxNode? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static VariableReferenceExpressionSyntaxNode? TryRead(Reader reader)
    {
        return reader.WithCache(nameof(VariableReferenceExpressionSyntaxNodeParser), static r =>
        {
            if (TokenNodeParser.TryReadIdentifier(r, out var token))
                return VariableReferenceExpressionSyntaxNode.Create(token);
            return null;
        });
    }
}