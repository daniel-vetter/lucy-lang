using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using System.Collections.Generic;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration
{
    internal static class FunctionDeclarationModifierList
    {
        public static List<SyntaxElementBuilder> Read(Code code)
        {
            var l = new List<SyntaxElementBuilder>();

            while (true)
            {
                if (SyntaxElementParser.TryReadKeyword(code, "extern", out var externKeyword))
                {
                    l.Add(externKeyword);
                    continue;
                }

                if (SyntaxElementParser.TryReadKeyword(code, "cdecl", out var cdeclKeyword))
                {
                    l.Add(cdeclKeyword);
                    continue;
                }

                if (SyntaxElementParser.TryReadKeyword(code, "stdcall", out var stdcallKeyword))
                {
                    l.Add(stdcallKeyword);
                    continue;
                }

                break;
            }

            return l;
        }
    }
}
