using System.Collections.Generic;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions.Nested;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Expressions;

public abstract class ExpressionSyntaxNodeParser
{
    public static ExpressionSyntaxNodeBuilder Missing(string? errorMessage = null)
    {
        var node = new MissingExpressionSyntaxNodeBuilder();
        if (errorMessage != null)
            node.SyntaxErrors = new List<string> { errorMessage };
        return node;
    }

    public static bool TryRead(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNodeBuilder? result) 
        => IfExpressionSyntaxNodeParser.TryReadOrInner(reader, out result);
}