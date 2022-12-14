using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

internal static class UnaryExpression
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static ExpressionSyntaxNodeBuilder? TryRead(Reader reader)
    {
        return reader.WithCache<ExpressionSyntaxNodeBuilder?>(nameof(UnaryExpression), static code =>
        {
            if (FunctionCallExpressionSyntaxNodeParser.TryRead(code, out var functionCallExpressionSyntaxNode))
                return functionCallExpressionSyntaxNode;

            if (StringConstantExpressionSyntaxNodeParser.TryRead(code, out var stringConstantExpressionSyntaxNode))
                return stringConstantExpressionSyntaxNode;

            if (NumberConstantExpressionSyntaxNodeParser.TryRead(code, out var numberConstantExpressionSyntaxNode))
                return numberConstantExpressionSyntaxNode;

            if (VariableReferenceExpressionSyntaxNodeParser.TryRead(code, out var variableReferenceExpressionSyntaxNode))
                return variableReferenceExpressionSyntaxNode;

            return null;
        });
    }
}