using Lucy.Core.Model;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration
{
    public class VariableNameWithTypeDeclarationSyntaxNodeParser
    {
        public static bool Read(Code code, [NotNullWhen(true)] out VariableNameWithTypeDeclarationSyntaxNode? result)
        {
            var start = code.Position;
            result = null;

            if (!SyntaxElementParser.TryReadIdentifier(code, out var variableName))
                return false;

            if (!SyntaxElementParser.TryReadExact(code, ":", out var seperator))
            {
                code.SeekTo(start);
                return false;
            }

            if (!TypeReferenceSyntaxNodeParser.TryRead(code, out var typeReference))
            {
                code.SeekTo(start);
                return false;
            }

            result = new VariableNameWithTypeDeclarationSyntaxNode(variableName, seperator, typeReference);
            return true;
        }
    }
}
