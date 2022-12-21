using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class TypeReferenceSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out TypeReferenceSyntaxNode? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static TypeReferenceSyntaxNode? TryRead(Reader reader)
    {
        return reader.WithCache(nameof(TypeReferenceSyntaxNodeParser), static r =>
        {
            if (!TokenNodeParser.TryReadIdentifier(r, out var token))
                return null;

            return TypeReferenceSyntaxNode.Create(token);
        });
    }

    public static TypeReferenceSyntaxNode Missing(string? errorMessage = null)
    {
        return new TypeReferenceSyntaxNode(null!, TokenNodeParser.Missing(errorMessage), default);
    }
}