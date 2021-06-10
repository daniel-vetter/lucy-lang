using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration
{
    public class TypeReferenceSyntaxNode : SyntaxTreeNode
    {
        public TypeReferenceSyntaxNode(SyntaxElement typeName)
        {
            TypeName = typeName;
        }

        public SyntaxElement TypeName { get; set; }

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
