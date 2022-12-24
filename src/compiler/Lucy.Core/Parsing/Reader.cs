using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Lucy.Core.Model;
using Lucy.Core.ProjectManagement;

namespace Lucy.Core.Parsing;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Reader
{
    private readonly string _code;
    private readonly Dictionary<CacheKey, CacheEntry> _cache = new();

    private int _position;
    private int _maxPeek;

    public Reader(string code)
    {
        _code = code;
    }

    private Reader(string code, Dictionary<CacheKey, CacheEntry> cache)
    {
        _code = code;
        _cache = cache;
    }

    public string Code => _code;

    public Reader Update(Range1D range, string newContent, out ImmutableArray<object> removedFromCache)
    {
        // Update the actual code
        var sb = new StringBuilder();
        sb.Append(_code[..range.Start.Position]);
        sb.Append(newContent);
        sb.Append(_code[(range.End.Position)..]);
        var code = sb.ToString();
        
        // Update the cache
        var newCache = new Dictionary<CacheKey, CacheEntry>();
        var removed = ImmutableArray.CreateBuilder<object>();

        // How much the range will grow/shrink with the new content
        var lengthDifference = newContent.Length - range.Length;

        foreach (var (key, entry) in _cache)
        {
            if (key.StartPosition >= range.End.Position)
            {
                // If the cache entry is after the change, we push it further back
                var movedKey = key with
                {
                    StartPosition = key.StartPosition + lengthDifference
                };

                var movedEntry = entry with
                {
                    EndPosition = entry.EndPosition + lengthDifference,
                    MaxPeek = entry.MaxPeek + lengthDifference
                };

                newCache[movedKey] = movedEntry;
            }
            else if (new Range1D(key.StartPosition, entry.MaxPeek).IntersectsWith(range))
            {
                // If the cache entry intersects with the change, we throw it away.
                // This is the range where the parser needs to reparse the code.
                // This method returns a list of all removed cache entries, so we still need to keep track of it.
                removed.Add(entry.Result);
            }
            else
            {
                // In this case, the cache entry is before the changed range, so we just copy it to the new dictionary.
                newCache[key] = entry;
            }
        }

        removedFromCache = removed.ToImmutable();

        return new Reader(code, newCache);
    }

    private string VisCache(Dictionary<CacheKey, CacheEntry> cache)
    {
        var sb = new StringBuilder();
        foreach (var cacheKey in cache.Keys.OrderBy(x => x.StartPosition).ThenBy(x => cache[x].MaxPeek))
        {
            var v = cache[cacheKey];
            var text = v.Result switch
            {
                TokenNode t => "\"" + t.Text + t.TrailingTrivia + "\"",
                _ => v.Result.ToString()
            };
            sb.AppendLine($"{cacheKey.StartPosition}-{v.EndPosition}/{v.MaxPeek}: {cacheKey.Key} {text}");
        }
        return sb.ToString();
    }

    public string Read(int length)
    {
        _maxPeek = Math.Max(_maxPeek, _position + length + 1);
        var result = _code.Substring(_position, length);
        _position += length;
        return result;
    }

    public T WithCache<T>(object cacheKey, Func<Reader, T> handler)
    {
        var start = _position;

        // if the cache contains a entry, seek to the recorded end and return the cached node
        if (_cache.TryGetValue(new CacheKey(cacheKey, _position), out var entry))
        {
            _maxPeek = entry.MaxPeek;
            _position = entry.EndPosition;
            return (T)entry.Result;
        }

        // otherwise, the handler needs to parse a fresh node
        var result = handler(this);
        if (result == null)
        {
            // if this fails, recover the old state so some other parse method can have its try
            _position = start;
        }

        // record a new cache entry
        if (result != null)
            _cache[new CacheKey(cacheKey, start)] = new CacheEntry(_position, _maxPeek, result);

        return result;
    }

    public void Seek(int offset)
    {
        _maxPeek = Math.Max(_maxPeek, _position + offset + 1);
        _position += offset;
    }

    public char Peek(int offset = 0)
    {
        _maxPeek = Math.Max(_maxPeek, _position + offset + 1);
        return _position + offset < _code.Length ? _code[_position + offset] : '\0';
    }

    private string DebuggerDisplay => _code[_position..];

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record CacheKey(object Key, int StartPosition);
    private record CacheEntry(int EndPosition, int MaxPeek, object Result);
}