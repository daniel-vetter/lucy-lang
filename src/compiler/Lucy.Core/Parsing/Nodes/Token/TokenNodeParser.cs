using Lucy.Core.Model;
using System.Diagnostics.CodeAnalysis;
using Lucy.Core.Parsing.Nodes.Trivia;
using System.Collections.Immutable;

namespace Lucy.Core.Parsing.Nodes.Token;

public static class TokenNodeParser
{
    private const string _identifierCacheKey = "Identifier" + nameof(TokenNodeParser);

    public static TokenNode Missing(string? errorMessage = null)
    {
        return new TokenNode(null, "", null, errorMessage == null ? ImmutableArray<string>.Empty : ImmutableArray.Create(errorMessage));
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record TryReadExactCacheKey(string Text);
    public static bool TryReadExact(Reader reader, string text, [NotNullWhen(true)] out TokenNode? result)
    {
        result = TryReadExact(reader, text);
        return result != null;
    }

    public static TokenNode? TryReadExact(Reader reader, string text)
    {
        return reader.WithCache(new TryReadExactCacheKey(text), static (r, ck) =>
        {
            for (var i = 0; i < ck.Text.Length; i++)
                if (r.Peek(i) != ck.Text[i])
                    return null;

            r.Seek(ck.Text.Length);
            return TokenNode.Create(ck.Text, TriviaParser.Read(r));
        });
    }

    public static bool TryReadIdentifier(Reader reader, [NotNullWhen(true)] out TokenNode? result)
    {
        result = TryReadIdentifier(reader);
        return result != null;
    }

    public static TokenNode? TryReadIdentifier(Reader reader)
    {
        return reader.WithCache(_identifierCacheKey, static (r, _) =>
        {
            var length = 0;
            while (IsIdentifierChar(r.Peek(length), length == 0))
                length++;

            return length == 0
                ? null
                : TokenNode.Create(r.Read(length), TriviaParser.Read(r));
        });
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record TryReadKeywordCacheKey(string Keyword);
    public static bool TryReadKeyword(Reader reader, string keyword, [NotNullWhen(true)] out TokenNode? result)
    {
        result = TryReadKeyword(reader, keyword);
        return result != null;
    }

    public static TokenNode? TryReadKeyword(Reader reader, string keyword)
    {
        return reader.WithCache(new TryReadKeywordCacheKey(keyword), static (r, ck) =>
        {
            for (var i = 0; i < ck.Keyword.Length; i++)
            {
                if (r.Peek(i) == ck.Keyword[i])
                    continue;
                return null;
            }

            var nextChar = r.Peek(ck.Keyword.Length);
            if (IsIdentifierChar(nextChar, false))
                return null;

            r.Seek(ck.Keyword.Length);
            return TokenNode.Create(ck.Keyword, TriviaParser.Read(r));
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