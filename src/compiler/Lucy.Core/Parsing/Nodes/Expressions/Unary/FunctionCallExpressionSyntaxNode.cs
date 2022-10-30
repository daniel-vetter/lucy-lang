using Lucy.Core.Parsing.Nodes.Token;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary
{
    public record FunctionCallExpressionSyntaxNode(SyntaxElement FunctionName, SyntaxElement OpenBraket, ComparableReadOnlyList<FunctionCallArgumentSyntaxNode> ArgumentList, SyntaxElement CloseBraket) : ExpressionSyntaxNode
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out FunctionCallExpressionSyntaxNode? result)
        {
            using var t = code.BeginTransaction();
            result = null;

            if (!SyntaxElement.TryReadIdentifier(code, out var functionName))
                return false;

            if (!SyntaxElement.TryReadExact(code, "(", out var openBraket))
                return false;

            t.Commit();

            var argumentList = FunctionCallArgumentSyntaxNode.Read(code);

            if (!SyntaxElement.TryReadExact(code, ")", out var closeBraket))
                closeBraket = SyntaxElement.Synthesize();

            result = new FunctionCallExpressionSyntaxNode(functionName, openBraket, argumentList, closeBraket);
            return true;
        }
    }
}
