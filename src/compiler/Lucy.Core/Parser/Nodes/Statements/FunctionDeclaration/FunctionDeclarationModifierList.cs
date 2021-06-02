using Lucy.Core.Parser.Nodes.Token;
using System.Collections.Generic;

namespace Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration
{
    internal static class FunctionDeclarationModifierList
    {
        public static List<SyntaxElement> Read(Code code)
        {
            var l = new List<SyntaxElement>();

            while (true)
            {
                if (SyntaxElement.TryReadKeyword(code, "extern", out var externKeyword))
                {
                    l.Add(externKeyword);
                    continue;
                }

                if (SyntaxElement.TryReadKeyword(code, "cdecl", out var cdeclKeyword))
                {
                    l.Add(cdeclKeyword);
                    continue;
                }

                if (SyntaxElement.TryReadKeyword(code, "stdcall", out var stdcallKeyword))
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
