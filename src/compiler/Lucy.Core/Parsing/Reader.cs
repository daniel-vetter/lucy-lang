using Lucy.Core.Model;
using Lucy.Core.ProjectManagement;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Lucy.Core.Parsing;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Reader
{
    private readonly string _code;
    private readonly Dictionary<CacheKey, CacheEntry> _cache = new();

    private int _position;
    private int _maxPeek;

    private readonly int _lastNodeId;
    private readonly string _documentPath;

    public Reader(string documentPath, string code)
    {
        _code = code;
        _documentPath = documentPath;
    }

    private Reader(string documentPath, string code, Dictionary<CacheKey, CacheEntry> cache, int lastNodeId)
    {
        _code = code;
        _cache = cache;
        _documentPath = documentPath;
        _lastNodeId = lastNodeId;
    }

    public string Code => _code;

    public Reader Update(Range1D range, string newContent, out ImmutableArray<object> removedFromCache)
    {
        var start = range.Start.Position;
        var rangeLength = range.End.Position - range.Start.Position;

        var sb = new StringBuilder();
        sb.Append(_code[..start]);
        sb.Append(newContent);
        sb.Append(_code[(start + rangeLength)..]);
        var code = sb.ToString();

        // TODO: Fix the text was removed. Current this code only supports adding content.

        var cache = new Dictionary<CacheKey, CacheEntry>();
        var removed = ImmutableArray.CreateBuilder<object>();
        if (newContent.Length > rangeLength)
        {
            var added = newContent.Length - rangeLength;
            var overwritten = newContent.Length - added;
            foreach (var (key, entry) in _cache)
            {
                if (key.StartPosition >= start + overwritten)
                {
                    cache[key with { StartPosition = key.StartPosition + added }] = entry with { EndPosition = entry.EndPosition + added, MaxPeek = entry.MaxPeek + added };
                }
                else if ((key.StartPosition >= start && key.StartPosition < start + overwritten) ||
                         (entry.MaxPeek >= start && entry.MaxPeek < start + overwritten) ||
                         (key.StartPosition <= start && entry.MaxPeek >= start + overwritten))
                {
                    if (entry.Result != null)
                        removed.Add(entry.Result);
                }
                else
                    cache[key] = entry;
            }
        }

        removedFromCache = removed.ToImmutable();

        return new Reader(_documentPath, code, cache, _lastNodeId);
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
            _maxPeek = entry.MaxPeek;
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
        if (result != null)  //TODO: Should we only cache positive results? (if so, CacheEntry.Result can be made non nullable)
            _cache[new CacheKey(cacheKey, start)] = new CacheEntry(_position, _maxPeek, result);

        return result;
    }

    public void Seek(int offset) => _position += offset;
    public char Peek(int offset = 0)
    {
        _maxPeek = Math.Max(_maxPeek, _position + offset);
        return _position + offset < _code.Length ? _code[_position + offset] : '\0';
    }

    private string DebuggerDisplay => _code[_position..];

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record CacheKey(object Key, int StartPosition);
    private record CacheEntry(int EndPosition, int MaxPeek, object? Result);
}