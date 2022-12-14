using Lucy.Core.ProjectManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Lucy.Core.Parsing;

[EditorBrowsable(EditorBrowsableState.Never)]
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

    public Reader Update(Range1D range, string newContent)
    {
        var start = range.Start.Position;
        var length = range.End.Position - range.Start.Position;

        var sb = new StringBuilder();
        sb.Append(_code[..start]);
        sb.Append(newContent);
        sb.Append(_code[(start + length)..]);
        var code = sb.ToString();

        var cache = new Dictionary<CacheKey, CacheEntry>();
        var diff = length - newContent.Length;
        foreach (var (key, entry) in _cache)
        {
            if (key.StartPosition < start + length && start < key.StartPosition + entry.PeekedLength)
                continue;
            
            cache[key.StartPosition >= start 
                ? key with { StartPosition = key.StartPosition + diff } 
                : key] = entry;
        }

        return new Reader(code, cache);
    }

    public string Read(int length)
    {
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
            _position = entry.EndPosition;
            return (T)entry.Result!;
        }

        // otherwise, the handler needs to parse a fresh node
        var result = handler(this);
        if (result == null)
        {
            // if this fails, recover the old state so some other parse method can have its try
            _position = start;
        }

        // record a new cache entry
        _cache[new CacheKey(cacheKey, start)] = new CacheEntry(_position, _maxPeek - start, result);

        return result;
    }

    public void Seek(int offset) => _position += offset;
    public char Peek(int offset = 0)
    {
        _maxPeek = Math.Max(_maxPeek, _position + offset);
        return _position + offset < _code.Length ? _code[_position + offset] : '\0';
    }

    private string DebuggerDisplay => _code[_position..];


    private record CacheKey(object Key, int StartPosition);
    private record CacheEntry(int EndPosition, int PeekedLength, object? Result);
}
