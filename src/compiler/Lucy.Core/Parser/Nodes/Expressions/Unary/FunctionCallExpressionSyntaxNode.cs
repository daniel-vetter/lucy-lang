using Lucy.Core.Model;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Token;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Expressions.Unary
{
    public class FunctionCallExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public FunctionCallExpressionSyntaxNode(TokenNode functionName, TokenNode openBraket, List<FunctionCallArgumentSyntaxNode> argumentList, TokenNode closeBraket)
        {
            FunctionName = functionName;
            OpenBraket = openBraket;
            ArgumentList = argumentList;
            CloseBraket = closeBraket;
        }

        public TokenNode FunctionName { get; }
        public TokenNode OpenBraket { get; }
        public List<FunctionCallArgumentSyntaxNode> ArgumentList { get; }
        public TokenNode CloseBraket { get; }

        public static bool TryRead(Code code, [NotNullWhen(true)] out FunctionCallExpressionSyntaxNode? result)
        {
            result = null;

            if (!TokenNode.TryReadIdentifier(code, out var functionName))
                return false;

            if (!TokenNode.TryReadExact(code, "(", out var openBraket))
                return false;

            var argumentList = FunctionCallArgumentSyntaxNode.Read(code);

            if (!TokenNode.TryReadExact(code, ")", out var closeBraket))
                return false;

            result = new FunctionCallExpressionSyntaxNode(functionName, openBraket, argumentList, closeBraket);
            return true;
        }
    }
}
