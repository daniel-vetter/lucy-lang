using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary
{
    public class FunctionCallExpressionSyntaxNodeParser
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out FunctionCallExpressionSyntaxNode? result)
        {
            using var t = code.BeginTransaction();
            result = null;

            if (!SyntaxElementParser.TryReadIdentifier(code, out var functionName))
                return false;

            if (!SyntaxElementParser.TryReadExact(code, "(", out var openBraket))
                return false;

            t.Commit();

            var argumentList = FunctionCallArgumentSyntaxNodeParser.Read(code);

            if (!SyntaxElementParser.TryReadExact(code, ")", out var closeBraket))
                closeBraket = SyntaxElementParser.Missing("Expected ')'.");

            result = new FunctionCallExpressionSyntaxNode(functionName, openBraket, argumentList, closeBraket);
            return true;
        }
    }
}
