using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Stuff;

public static class VariableDefinitionSyntaxNodeParser
{
    public static bool TryReadVariableDefinitionSyntaxNode(Reader reader, [NotNullWhen(true)] out VariableDefinitionSyntaxNode? result)
    {
        result = Read(reader);
        return result != null;
    }

    public static VariableDefinitionSyntaxNode? Read(Reader reader)
    {
        return reader.WithCache(nameof(VariableDefinitionSyntaxNodeParser), static (r, _) =>
        {
            if (!TokenNodeParser.TryReadIdentifier(r, out var variableName))
                return null;

            TypeAnnotationSyntaxNodeParser.TryRead(r, out var variableType);

            return VariableDefinitionSyntaxNode.Create(variableName, variableType);
        });
    }

    public static VariableDefinitionSyntaxNode Missing(string errorMessage)
    {
        return new VariableDefinitionSyntaxNode(null, TokenNodeParser.Missing(), TypeAnnotationSyntaxNodeParser.Missing(), ImmutableArray.Create(errorMessage));
    }
}