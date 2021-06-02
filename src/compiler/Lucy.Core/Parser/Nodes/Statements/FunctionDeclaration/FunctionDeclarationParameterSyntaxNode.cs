using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Token;
using System.Collections.Generic;

namespace Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration
{
    public class FunctionDeclarationParameterSyntaxNode : SyntaxTreeNode
    {
        public FunctionDeclarationParameterSyntaxNode(VariableNameWithTypeDeclarationSyntaxNode parameter, SyntaxElement? seperator)
        {
            Parameter = parameter;
            Seperator = seperator;
        }

        public VariableNameWithTypeDeclarationSyntaxNode Parameter { get; set; }
        public SyntaxElement? Seperator { get; set; }

        public static List<FunctionDeclarationParameterSyntaxNode> ReadList(Code code)
        {
            var l = new List<FunctionDeclarationParameterSyntaxNode>();
            while (true)
            {
                if (!VariableNameWithTypeDeclarationSyntaxNode.Read(code, out var variableNameWithTypeDeclaration))
                    break;

                SyntaxElement.TryReadExact(code, ",", out var seperator);

                l.Add(new FunctionDeclarationParameterSyntaxNode(variableNameWithTypeDeclaration, seperator));

                if (seperator == null)
                    break;
            }

            return l;
        }
    }
}
