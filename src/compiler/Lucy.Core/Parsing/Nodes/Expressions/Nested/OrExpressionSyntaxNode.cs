using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested;

internal class OrExpressionSyntaxNodeParser
{
    public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result)
    {
        if (!AdditionExpressionSyntaxNodeParser.TryReadOrInner(code, out result))
            return false;
            
        while (true)
        {
            if (!SyntaxElementParser.TryReadKeyword(code, "or", out var orToken))
            {
                return true;
            }
                
            if (!AdditionExpressionSyntaxNodeParser.TryReadOrInner(code, out var right))
            {
                result = new OrExpressionSyntaxNodeBuilder(result, orToken, ExpressionSyntaxNodeParser.Missing("Expression expected"));
                return true;
            }

            result = new OrExpressionSyntaxNodeBuilder(result, orToken, right);
        }
    }
}