using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Expressions.Unary
{
    internal class UnaryExpression
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            if (FunctionCallExpressionSyntaxNode.TryRead(code, out var functionCallExpressionSyntaxNode))
            {
                result = functionCallExpressionSyntaxNode;
                return true;
            }
            
            if (StringConstantExpressionSyntaxNode.TryRead(code, out var stringConstantExpressionSyntaxNode))
            {
                result = stringConstantExpressionSyntaxNode;
                return true;
            }

            if (NumberConstantExpressionSyntaxNode.TryRead(code, out var numberConstantExpressionSyntaxNode))
            {
                result = numberConstantExpressionSyntaxNode;
                return true;
            }
            
            if (VariableReferenceExpressionSyntaxNode.TryRead(code, out var variableReferenceExpressionSyntaxNode))
            {
                result = variableReferenceExpressionSyntaxNode;
                return true;
            }

            result = null;
            return false;
        }
    }
}
