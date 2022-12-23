using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions.Nested;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;

namespace Lucy.Core.Parsing.Nodes.Expressions;

public abstract class ExpressionSyntaxNodeParser
{
    public static ExpressionSyntaxNode Missing(string? errorMessage = null)
    {
        // TODO: Must also be cached so the id stays stable
        return new MissingExpressionSyntaxNode(
            nodeId: null,
            syntaxErrors: errorMessage == null ? ImmutableArray<string>.Empty : ImmutableArray.Create(errorMessage)
        );
    }

    public static bool TryRead(Reader reader, [NotNullWhen(true)] out ExpressionSyntaxNode? result) 
        => IfExpressionSyntaxNodeParser.TryReadOrInner(reader, out result);
}