using Lucy.Core.Parsing.Nodes.Expressions.Nested;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions
{
    public abstract record ExpressionSyntaxNode : SyntaxTreeNode
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result) => IfExpressionSyntaxNode.TryReadOrInner(code, out result);
    }
}
