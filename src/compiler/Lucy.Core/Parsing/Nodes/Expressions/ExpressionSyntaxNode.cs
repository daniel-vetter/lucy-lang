using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions.Nested;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions
{
    public abstract class ExpressionSyntaxNodeParser
    {
        public static ExpressionSyntaxNodeBuilder Missing(string? errorMessage = null)
        {
            var node = new MissingExpressionSyntaxNodeBuilder();
            if (errorMessage != null)
                node.SyntaxErrors.Add(errorMessage);
            return node;
        }

        public static bool TryRead(Code code, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result) => IfExpressionSyntaxNodeParser.TryReadOrInner(code, out result);
    }
}
