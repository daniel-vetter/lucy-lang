using Lucy.Core.Parsing.Nodes.Token;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public record FunctionDeclarationParameterSyntaxNode(VariableNameWithTypeDeclarationSyntaxNode VariableDeclaration, SyntaxElement? Seperator) : SyntaxTreeNode
{
    public static ComparableReadOnlyList<FunctionDeclarationParameterSyntaxNode> ReadList(Code code)
    {
        var l = new ComparableReadOnlyList<FunctionDeclarationParameterSyntaxNode>.Builder();
        while (true)
        {
            if (!VariableNameWithTypeDeclarationSyntaxNode.Read(code, out var variableNameWithTypeDeclaration))
                break;

            SyntaxElement.TryReadExact(code, ",", out var seperator);

            l.Add(new FunctionDeclarationParameterSyntaxNode(variableNameWithTypeDeclaration, seperator));

            if (seperator == null)
                break;
        }

        return l.Build();
    }
}
