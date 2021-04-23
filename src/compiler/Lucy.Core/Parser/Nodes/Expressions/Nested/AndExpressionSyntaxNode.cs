using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Expressions.Nested
{
    internal class AndExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public AndExpressionSyntaxNode(ExpressionSyntaxNode left, TokenNode andKeyword, ExpressionSyntaxNode right)
        {
            Left = left;
            AndKeyword = andKeyword;
            Right = right;
        }

        public ExpressionSyntaxNode Left { get; }
        public TokenNode AndKeyword { get; }
        public ExpressionSyntaxNode Right { get; }

        public static bool TryReadOrInner(Code code, [NotNullWhen(true)] out ExpressionSyntaxNode? result)
        {
            if (!OrExpressionSyntaxNode.TryReadOrInner(code, out result))
                return false;

            while (true)
            {
                if (!TokenNode.TryReadKeyword(code, "and", out var andToken))
                    return true;

                if (!OrExpressionSyntaxNode.TryReadOrInner(code, out var right))
                {
                    code.ReportError("Expected expression", code.Position);
                    return true;
                }

                result = new AndExpressionSyntaxNode(result, andToken, right);
            }
        }
    }
}
