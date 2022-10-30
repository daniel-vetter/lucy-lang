using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration
{
    public record VariableNameWithTypeDeclarationSyntaxNode(SyntaxElement VariableName, SyntaxElement Seperator, TypeReferenceSyntaxNode TypeReference) : SyntaxTreeNode
    {
        public static bool Read(Code code, [NotNullWhen(true)] out VariableNameWithTypeDeclarationSyntaxNode? result)
        {
            var start = code.Position;
            result = null;

            if (!SyntaxElement.TryReadIdentifier(code, out var variableName))
                return false;

            if (!SyntaxElement.TryReadExact(code, ":", out var seperator))
            {
                code.SeekTo(start);
                return false;
            }

            if (!TypeReferenceSyntaxNode.TryRead(code, out var typeReference))
            {
                code.SeekTo(start);
                return false;
            }

            result = new VariableNameWithTypeDeclarationSyntaxNode(variableName, seperator, typeReference);
            return true;
        }
    }
}
