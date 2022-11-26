using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary;

public class VariableReferenceExpressionSyntaxNodeParser
{
    public static bool TryRead(Code code, [NotNullWhen(true)] out VariableReferenceExpressionSyntaxNodeBuilder? result)
    {
        if (SyntaxElementParser.TryReadIdentifier(code, out var token))
        {
            result = new VariableReferenceExpressionSyntaxNodeBuilder(token);
            return true;
        }
            
        result = null;
        return false;
    }
}