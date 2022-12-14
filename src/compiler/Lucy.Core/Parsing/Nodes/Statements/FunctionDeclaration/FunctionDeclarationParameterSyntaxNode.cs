using System.Collections.Generic;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class FunctionDeclarationParameterSyntaxNodeParser 
{
    private const string _listCacheKey = "List" + nameof(FunctionDeclarationParameterSyntaxNodeParser);

    public static List<FunctionDeclarationParameterSyntaxNodeBuilder> Read(Reader reader)
    {
        return reader.WithCache(_listCacheKey, static code =>
        {
            var l = new List<FunctionDeclarationParameterSyntaxNodeBuilder>();
            while (true)
            {
                var next = code.WithCache(nameof(FunctionDeclarationParameterSyntaxNodeParser), static code =>
                {
                    if (!TokenNodeParser.TryReadIdentifier(code, out var variableName))
                        return null;

                    if (!TypeAnnotationSyntaxNodeParser.TryRead(code, out var variableType))
                        variableType = TypeAnnotationSyntaxNodeParser.Missing("Parameter type expected");

                    TokenNodeParser.TryReadExact(code, ",", out var separator);

                    return new FunctionDeclarationParameterSyntaxNodeBuilder(variableName, variableType, separator);
                });

                if (next == null)
                    break;
                
                l.Add(next);

                if (next.Seperator == null)
                    break;
            }

            return l;
        });
    }
}