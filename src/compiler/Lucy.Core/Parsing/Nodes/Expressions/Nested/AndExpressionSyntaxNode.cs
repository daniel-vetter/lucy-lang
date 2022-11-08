using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Nested
{
    internal class AndExpressionSyntaxNodeParser
    {
        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            if (!OrExpressionSyntaxNodeParser.TryReadOrInner(code, out result))
                return false;

            while (true)
            {
                if (!SyntaxElementParser.TryReadKeyword(code, "and", out var andToken))
                    return true;

                if (!OrExpressionSyntaxNodeParser.TryReadOrInner(code, out var right))
                {
                    right = new MissingExpressionSyntaxNode()
                    {
                        SyntaxErrors = { { "Expected expression" } }
                    };
                    return true;
                }

                result = new AndExpressionSyntaxNode(result, andToken, right);
            }
        }
    }
}
