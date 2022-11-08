using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions.Nested;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions
{
    public abstract class ExpressionSyntaxNodeParser : SyntaxTreeNode
    {
        public static ExpressionSyntaxNode Missing(string? errorMessage = null)
        {
            var node = new MissingExpressionSyntaxNode();
            if (errorMessage != null)
                node.SyntaxErrors.Add(errorMessage);
            return node;
        }

        public static bool TryRead(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result) => IfExpressionSyntaxNodeParser.TryReadOrInner(code, out result);
    }
}
