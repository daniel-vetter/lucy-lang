using Lucy.Core.Parser.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Expressions.Nested
{
    public class IfExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public IfExpressionSyntaxNode(ExpressionSyntaxNode condition, SyntaxElement ifToken, ExpressionSyntaxNode thenExpression, SyntaxElement elseToken, ExpressionSyntaxNode elseExpression)
        {
            Condition = condition;
            IfToken = ifToken;
            ThenExpression = thenExpression;
            ElseToken = elseToken;
            ElseExpression = elseExpression;
        }

        public ExpressionSyntaxNode Condition { get; }
        public SyntaxElement IfToken { get; }
        public ExpressionSyntaxNode ThenExpression { get; }
        public SyntaxElement ElseToken { get; }
        public ExpressionSyntaxNode ElseExpression { get; }

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
