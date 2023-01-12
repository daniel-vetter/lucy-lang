using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.Parsing.Nodes.Token;

namespace Lucy.Core.Parsing.Nodes.Stuff;

public static class TypeAnnotationSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out TypeAnnotationSyntaxNode? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static TypeAnnotationSyntaxNode? TryRead(Reader reader)
    {
        return reader.WithCache(nameof(TypeAnnotationSyntaxNodeParser), static r =>
        {
            if (!TokenNodeParser.TryReadExact(r, ":", out var separator))
                return null;

            if (!TypeReferenceSyntaxNodeParser.TryRead(r, out var typeReference))
                typeReference = TypeReferenceSyntaxNodeParser.Missing("Type expected");

            return TypeAnnotationSyntaxNode.Create(separator, typeReference);
        });
    }

    public static TypeAnnotationSyntaxNode Missing(string? errorMessage = null)
    {
        return new TypeAnnotationSyntaxNode(null, TokenNodeParser.Missing(errorMessage), TypeReferenceSyntaxNodeParser.Missing(), ImmutableArray<string>.Empty);
    }
}