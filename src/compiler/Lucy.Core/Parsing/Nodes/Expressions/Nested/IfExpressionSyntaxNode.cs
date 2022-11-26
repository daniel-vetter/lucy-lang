using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

public class IfExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result)
    {
        if (!AndExpressionSyntaxNodeParser.TryReadOrInner(code, out result))
            return false;

        while (true)
        {
            if (!SyntaxElementParser.TryReadExact(code, "?", out var ifToken))
                return true;

            if (!AndExpressionSyntaxNodeParser.TryReadOrInner(code, out var thenExpression))
                thenExpression = ExpressionSyntaxNodeParser.Missing("Expected expression after '?'");

            if (!SyntaxElementParser.TryReadExact(code, ":", out var elseToken))
                elseToken = SyntaxElementParser.Missing("Expected ':'");

            if (!AndExpressionSyntaxNodeParser.TryReadOrInner(code, out var elseExpression))
                elseExpression = ExpressionSyntaxNodeParser.Missing("Expression exptected after ':'");

            result = new IfExpressionSyntaxNodeBuilder(result, ifToken, thenExpression, elseToken, elseExpression);
        }
    }
}