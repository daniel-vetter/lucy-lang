using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class TypeAnnotationSyntaxNodeParser
{
    public static bool TryRead(Reader reader, [NotNullWhen(true)] out TypeAnnotationSyntaxNodeBuilder? result)
    {
        result = TryRead(reader);
        return result != null;
    }

    public static TypeAnnotationSyntaxNodeBuilder? TryRead(Reader reader)
    {
        return reader.WithCache(nameof(TypeAnnotationSyntaxNodeParser), static code =>
        {
            if (!TokenNodeParser.TryReadExact(code, ":", out var separator))
                return null;
            
            if (!TypeReferenceSyntaxNodeParser.TryRead(code, out var typeReference))
                typeReference = TypeReferenceSyntaxNodeParser.Missing("Type expected");

            return new TypeAnnotationSyntaxNodeBuilder(separator, typeReference);
        });
    }

    public static TypeAnnotationSyntaxNodeBuilder Missing(string? errorMessage = null)
    {
        return new TypeAnnotationSyntaxNodeBuilder(TokenNodeParser.Missing(errorMessage), TypeReferenceSyntaxNodeParser.Missing());
    }
}