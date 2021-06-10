using Lucy.Core.Parser.Nodes.Token;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Expressions.Unary
{
    public class FunctionCallExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public FunctionCallExpressionSyntaxNode(SyntaxElement functionName, SyntaxElement openBraket, List<FunctionCallArgumentSyntaxNode> argumentList, SyntaxElement closeBraket)
        {
            FunctionName = functionName;
            OpenBraket = openBraket;
            ArgumentList = argumentList;
            CloseBraket = closeBraket;
        }

        public SyntaxElement FunctionName { get; }
        public SyntaxElement OpenBraket { get; }
        public List<FunctionCallArgumentSyntaxNode> ArgumentList { get; }
        public SyntaxElement CloseBraket { get; }

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
