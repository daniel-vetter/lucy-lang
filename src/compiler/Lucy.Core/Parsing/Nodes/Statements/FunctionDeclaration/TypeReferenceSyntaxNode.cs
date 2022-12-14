using Lucy.Core.Model;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class TypeReferenceSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out TypeReferenceSyntaxNodeBuilder? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static TypeReferenceSyntaxNodeBuilder? TryRead(Reader reader)
    {
        return reader.WithCache(nameof(TypeReferenceSyntaxNodeParser), static code =>
        {
            if (!TokenNodeParser.TryReadIdentifier(code, out var token))
                return null;

            return new TypeReferenceSyntaxNodeBuilder(token);
        });
    }

    public static TypeReferenceSyntaxNodeBuilder Missing(string? errorMessage = null)
    {
        return new TypeReferenceSyntaxNodeBuilder(TokenNodeParser.Missing(errorMessage));
    }
}