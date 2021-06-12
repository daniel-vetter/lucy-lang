using Lucy.Core.Parsing.Nodes.Token;
using System.Collections.Generic;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration
{
    public class FunctionDeclarationParameterSyntaxNode : SyntaxTreeNode
    {
        public FunctionDeclarationParameterSyntaxNode(VariableNameWithTypeDeclarationSyntaxNode variableDeclaration, SyntaxElement? seperator)
        {
            VariableDeclaration = variableDeclaration;
            Seperator = seperator;
        }

        public VariableNameWithTypeDeclarationSyntaxNode VariableDeclaration { get; set; }
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
