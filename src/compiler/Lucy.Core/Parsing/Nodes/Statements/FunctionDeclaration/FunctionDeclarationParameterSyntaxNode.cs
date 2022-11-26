using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Collections.Generic;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public class FunctionDeclarationParameterSyntaxNodeParser 
{
    public static List<FunctionDeclarationParameterSyntaxNodeBuilder> ReadList(Code code)
    {
        var l = new List<FunctionDeclarationParameterSyntaxNodeBuilder>();
        while (true)
        {
            if (!VariableNameWithTypeDeclarationSyntaxNodeParser.Read(code, out var variableNameWithTypeDeclaration))
                break;

            SyntaxElementParser.TryReadExact(code, ",", out var seperator);

            l.Add(new FunctionDeclarationParameterSyntaxNodeBuilder(variableNameWithTypeDeclaration, seperator));

            if (seperator == null)
                break;
        }

        return l;
    }
}