using Lucy.Core.Parsing.Nodes.Token;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration
{
    internal static class FunctionDeclarationModifierList
    {
        public static ComparableReadOnlyList<SyntaxElement> Read(Code code)
        {
            var l = new ComparableReadOnlyList<SyntaxElement>.Builder();

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

            return l.Build();
        }
    }
}
