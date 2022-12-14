using System.Collections.Generic;
using Lucy.Core.Model;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Parsing.Nodes.Trivia;

namespace Lucy.Core.Parsing;

public static class TokenNodeParser
{
    private const string _identifierCacheKey = "Identifier" + nameof(TokenNodeParser);

    public static TokenNodeBuilder Missing(string? errorMessage = null)
    {
        var token = new TokenNodeBuilder("", null);
        if (errorMessage != null)
            token.SyntaxErrors = new List<string> { errorMessage };
        return token;
    }
    
    private record TryReadExactCacheKey(string Text);
    public static bool TryReadExact(Reader reader, string text, [NotNullWhen(true)] out TokenNodeBuilder? result)
    {
        result = TryReadExact(reader, text);
        return result != null;
    }

    public static TokenNodeBuilder? TryReadExact(Reader reader, string text)
    {
        return reader.WithCache(new TryReadExactCacheKey(text), _ =>
        {
            for (var i = 0; i < text.Length; i++)
                if (reader.Peek(i) != text[i])
                    return null;

            reader.Seek(text.Length);
            return new TokenNodeBuilder(text, TriviaParser.Read(reader));
        });
    }

    public static bool TryReadIdentifier(Reader reader, [NotNullWhen(true)] out TokenNodeBuilder? result)
    {
        result = TryReadIdentifier(reader);
        return result != null;
    }

    public static TokenNodeBuilder? TryReadIdentifier(Reader reader)
    {
        return reader.WithCache(_identifierCacheKey, static code =>
        {
            var length = 0;
            while (IsIdentifierChar(code.Peek(length), length == 0))
                length++;

            return length == 0 
                ? null 
                : new TokenNodeBuilder(code.Read(length), TriviaParser.Read(code));
        });
    }

    private record TryReadKeywordCacheKey(string Keyword);
    public static bool TryReadKeyword(Reader reader, string keyword, [NotNullWhen(true)] out TokenNodeBuilder? result)
    {
        result = TryReadKeyword(reader, keyword);
        return result != null;
    }

    public static TokenNodeBuilder? TryReadKeyword(Reader reader, string keyword)
    {
        return reader.WithCache(new TryReadKeywordCacheKey(keyword), code =>
        {
            for (var i = 0; i < keyword.Length; i++)
            {
                if (code.Peek(i) == keyword[i])
                    continue;
                return null;
            }

            var nextChar = code.Peek(keyword.Length);
            if (IsIdentifierChar(nextChar, false))
                return null;

            code.Seek(keyword.Length);
            return new TokenNodeBuilder(keyword, TriviaParser.Read(code));
        });
    }

    private static bool IsIdentifierChar(char ch, bool firstChar)
    {
        var isValid = ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';

        if (isValid || firstChar)
            return isValid;
        if (ch is >= '0' and <= '9' or '/')
            isValid = true;

        return isValid;
    }
}