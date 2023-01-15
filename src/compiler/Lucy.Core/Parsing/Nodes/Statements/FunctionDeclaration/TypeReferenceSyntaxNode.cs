using Lucy.Core.Model;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Parsing.Nodes.Stuff;

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
        return reader.WithCache(nameof(TypeReferenceSyntaxNodeParser), static (r, _) =>
        {
            if (!TokenNodeParser.TryReadIdentifier(r, out var token))
                return null;

            return TypeReferenceSyntaxNode.Create(token);
        });
    }

    public static TypeReferenceSyntaxNode Missing(string? errorMessage = null)
    {
        return new TypeReferenceSyntaxNode(null, TokenNodeParser.Missing(errorMessage), default);
    }
}