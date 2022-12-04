﻿using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class TypeReferenceSyntaxNodeParser
{
    public static bool TryRead(Code code, [NotNullWhen(true)] out TypeReferenceSyntaxNodeBuilder? result)
    {
        if (!SyntaxElementParser.TryReadIdentifier(code, out var token))
        {
            result = null;
            return false;
        }

        result = new TypeReferenceSyntaxNodeBuilder(token);
        return true;
    }

    internal static TypeReferenceSyntaxNodeBuilder Missing(string? errorMessage = null)
    {
        return new TypeReferenceSyntaxNodeBuilder(SyntaxElementParser.Missing(errorMessage));
    }
}