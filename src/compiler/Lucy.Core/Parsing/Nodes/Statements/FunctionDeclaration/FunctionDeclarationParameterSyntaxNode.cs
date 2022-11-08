using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Collections.Generic;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration
{
    public class FunctionDeclarationParameterSyntaxNodeParser 
    {
        public static List<FunctionDeclarationParameterSyntaxNode> ReadList(Code code)
        {
            var l = new List<FunctionDeclarationParameterSyntaxNode>();
            while (true)
            {
                if (!VariableNameWithTypeDeclarationSyntaxNodeParser.Read(code, out var variableNameWithTypeDeclaration))
                    break;

                SyntaxElementParser.TryReadExact(code, ",", out var seperator);

                l.Add(new FunctionDeclarationParameterSyntaxNode(variableNameWithTypeDeclaration, seperator));

                if (seperator == null)
                    break;
            }

            return l;
        }
    }
}
