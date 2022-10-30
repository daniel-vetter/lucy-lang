using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested
{
    public record IfExpressionSyntaxNode(ExpressionSyntaxNode Condition, SyntaxElement IfToken, ExpressionSyntaxNode ThenExpression, SyntaxElement ElseToken, ExpressionSyntaxNode ElseExpression) : ExpressionSyntaxNode
    {
        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            if (!AndExpressionSyntaxNode.TryReadOrInner(code, out result))
                return false;

            while (true)
            {
                if (!SyntaxElement.TryReadExact(code, "?", out var ifToken))
                    return true;

                if (!AndExpressionSyntaxNode.TryReadOrInner(code, out var thenExpression))
                {
                    code.ReportError("Expected expression after '?'", code.Position);
                    return false;
                }

                if (!SyntaxElement.TryReadExact(code, ":", out var elseToken))
                {
                    code.ReportError("Expected ':'", code.Position);
                    return false;
                }

                if (!AndExpressionSyntaxNode.TryReadOrInner(code, out var elseExpression))
                {
                    code.ReportError("Expression exptected after ':'", code.Position);
                    return false;
                }

                result = new IfExpressionSyntaxNode(result, ifToken, thenExpression, elseToken, elseExpression);
            }
        }
    }
}
