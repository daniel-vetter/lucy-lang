using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Expressions.Nested;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Expressions
{
    public abstract class ExpressionSyntaxNode : SyntaxTreeNode
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result) => IfExpressionSyntaxNode.TryReadOrInner(code, out result);
    }
}
