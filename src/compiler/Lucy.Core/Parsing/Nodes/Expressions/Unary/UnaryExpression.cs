using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

internal static class UnaryExpression
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static ExpressionSyntaxNode? TryRead(Reader reader)
    {
        return reader.WithCache<ExpressionSyntaxNode?, string>(nameof(UnaryExpression), static (r, _) =>
        {
            if (FunctionCallExpressionSyntaxNodeParser.TryRead(r, out var functionCallExpressionSyntaxNode))
                return functionCallExpressionSyntaxNode;

            if (StringConstantExpressionSyntaxNodeParser.TryRead(r, out var stringConstantExpressionSyntaxNode))
                return stringConstantExpressionSyntaxNode;

            if (NumberConstantExpressionSyntaxNodeParser.TryRead(r, out var numberConstantExpressionSyntaxNode))
                return numberConstantExpressionSyntaxNode;

            if (VariableReferenceExpressionSyntaxNodeParser.TryRead(r, out var variableReferenceExpressionSyntaxNode))
                return variableReferenceExpressionSyntaxNode;

            return null;
        });
    }
}