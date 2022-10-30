using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration
{
    public record TypeReferenceSyntaxNode(SyntaxElement typeName) : SyntaxTreeNode
    {
        public static bool TryRead(Code code, [NotNullWhen(true)] out TypeReferenceSyntaxNode? result)
        {
            if (!SyntaxElement.TryReadIdentifier(code, out var token))
            {
                result = null;
                return false;
            }

            result = new TypeReferenceSyntaxNode(token);
            return true;
        }

        internal static TypeReferenceSyntaxNode Synthesize(string? errorMessage = null)
        {
            return new TypeReferenceSyntaxNode(SyntaxElement.Synthesize(errorMessage));
        }
    }
}
