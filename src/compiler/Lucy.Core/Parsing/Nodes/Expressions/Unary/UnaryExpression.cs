using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

internal class UnaryExpression
{
    public static bool TryRead(Code code, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result)
    {
        if (FunctionCallExpressionSyntaxNodeParser.TryRead(code, out var functionCallExpressionSyntaxNode))
        {
            result = functionCallExpressionSyntaxNode;
            return true;
        }
            
        if (StringConstantExpressionSyntaxNodeParser.TryRead(code, out var stringConstantExpressionSyntaxNode))
        {
            result = stringConstantExpressionSyntaxNode;
            return true;
        }

        if (NumberConstantExpressionSyntaxNodeParser.TryRead(code, out var numberConstantExpressionSyntaxNode))
        {
            result = numberConstantExpressionSyntaxNode;
            return true;
        }
            
        if (VariableReferenceExpressionSyntaxNodeParser.TryRead(code, out var variableReferenceExpressionSyntaxNode))
        {
            result = variableReferenceExpressionSyntaxNode;
            return true;
        }

        result = null;
        return false;
    }
}