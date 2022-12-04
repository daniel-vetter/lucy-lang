using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Collections.Generic;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class FunctionDeclarationParameterSyntaxNodeParser 
{
    public static List<FunctionDeclarationParameterSyntaxNodeBuilder> ReadList(Code code)
    {
        var l = new List<FunctionDeclarationParameterSyntaxNodeBuilder>();
        while (true)
        {
            if (!SyntaxElementParser.TryReadIdentifier(code, out var variableName))
                break;

            if (!TypeAnnotationSyntaxNodeParser.TryRead(code, out var variableType))
                variableType = TypeAnnotationSyntaxNodeParser.Missing("Parameter type expected");
            
            SyntaxElementParser.TryReadExact(code, ",", out var separator);

            l.Add(new FunctionDeclarationParameterSyntaxNodeBuilder(variableName, variableType, separator));

            if (separator == null)
                break;
        }

        return l;
    }
}