using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class TypeAnnotationSyntaxNodeParser
{
    public static bool TryRead(Code code, [NotNullWhen(true)] out TypeAnnotationSyntaxNodeBuilder? result)
    {
        var t = code.BeginTransaction();

        if (!SyntaxElementParser.TryReadExact(code, ":", out var separator))
        {
            result = null;
            return false;
        }

        t.Commit();

        if (!TypeReferenceSyntaxNodeParser.TryRead(code, out var typeReference))
            typeReference = TypeReferenceSyntaxNodeParser.Missing("Type expected");

        result = new TypeAnnotationSyntaxNodeBuilder(separator, typeReference);
        return true;
    }

    public static TypeAnnotationSyntaxNodeBuilder Missing(string? errorMessage = null)
    {
        return new TypeAnnotationSyntaxNodeBuilder(SyntaxElementParser.Missing(errorMessage), TypeReferenceSyntaxNodeParser.Missing());
    }
}