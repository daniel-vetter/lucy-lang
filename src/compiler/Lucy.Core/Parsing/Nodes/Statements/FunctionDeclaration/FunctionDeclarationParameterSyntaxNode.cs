using System.Collections.Immutable;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class FunctionDeclarationParameterSyntaxNodeParser 
{
    private const string _listCacheKey = "List" + nameof(FunctionDeclarationParameterSyntaxNodeParser);

    public static ImmutableArray<FunctionDeclarationParameterSyntaxNode> Read(Reader reader)
    {
        return reader.WithCache(_listCacheKey, static code =>
        {
            var l = ImmutableArray.CreateBuilder<FunctionDeclarationParameterSyntaxNode>();
            while (true)
            {
                var next = code.WithCache(nameof(FunctionDeclarationParameterSyntaxNodeParser), static r =>
                {
                    if (!TokenNodeParser.TryReadIdentifier(r, out var variableName))
                        return null;

                    if (!TypeAnnotationSyntaxNodeParser.TryRead(r, out var variableType))
                        variableType = TypeAnnotationSyntaxNodeParser.Missing("Parameter type expected");

                    TokenNodeParser.TryReadExact(r, ",", out var separator);

                    return FunctionDeclarationParameterSyntaxNode.Create(variableName, variableType, separator);
                });

                if (next == null)
                    break;
                
                l.Add(next);

                if (next.Seperator == null)
                    break;
            }

            return l.ToImmutable();
        });
    }
}