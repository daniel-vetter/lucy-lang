using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested
{
    public class AdditionExpressionSyntaxNodeParser
    {
        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            if (!MemberAccessExpressionSyntaxNodeParser.TryReadOrInner(code, out result))
                return false;

            while (true)
            {
                if (!SyntaxElementParser.TryReadExact(code, "+", out var plusToken))
                    return true;

                if (!MemberAccessExpressionSyntaxNodeParser.TryReadOrInner(code, out var right))
                    right = ExpressionSyntaxNodeParser.Missing("Missing expression after '+'.");

                result = new AdditionExpressionSyntaxNode(result, plusToken, right);
            }
        }
    }
}
