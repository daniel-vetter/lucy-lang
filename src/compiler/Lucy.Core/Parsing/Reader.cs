﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Lucy.Core.ProjectManagement;

namespace Lucy.Core.Parsing;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Reader
{
    private readonly string _code;
    public readonly Dictionary<CacheKey, CacheEntry> _cache = new();
    private readonly HashSet<string> _internalizedStrings = new();

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

    public string Internalize(string str)
    {
        if (_internalizedStrings.TryGetValue(str, out var existing))
            return existing;

        _internalizedStrings.Add(str);
        return str;
    }

    public void Trim()
    {
        _cache.TrimExcess();
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

        // Rearrange the cache. This is a linear operation, should probably find some better algorithm for this.
        foreach (var (key, entry) in _cache)
        {
            if (key.StartPosition >= range.End.Position)
            {
                // If the cache entry is after the change, we push it further back
                var movedKey = new CacheKey(key.Key, key.StartPosition + lengthDifference);

                var movedEntry = new CacheEntry(entry.EndPosition + lengthDifference, entry.MaxPeek + lengthDifference, entry.Result);

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
    
    public string Read(int length)
    {
        _maxPeek = Math.Max(_maxPeek, _position + length + 1);
        var result = _code.Substring(_position, length);
        _position += length;
        return result;
    }
    
    public TResult WithCache<TResult, TCacheKey>(TCacheKey cacheKey, Func<Reader, TCacheKey, TResult> handler) where TCacheKey : notnull
    {
        var start = _position;

        // if the cache contains a entry, seek to the recorded end and return the cached node
        if (_cache.TryGetValue(new CacheKey(cacheKey, _position), out var entry))
        {
            _maxPeek = entry.MaxPeek;
            _position = entry.EndPosition;
            return (TResult)entry.Result;
        }

        // otherwise, the handler needs to parse a fresh node
        var result = handler(this, cacheKey);
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
    
    public readonly struct CacheKey
    {
        public object Key { get; }
        public int StartPosition { get; }

        public CacheKey(object key, int startPosition)
        {
            Key = key;
            StartPosition = startPosition;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, StartPosition);
        }

        public override bool Equals(object? obj)
        {
            return obj is CacheKey other && (Key.Equals(other.Key) && StartPosition == other.StartPosition);
        }
    }

    public readonly struct CacheEntry
    {
        public int EndPosition { get; }
        public int MaxPeek { get; }
        public object Result { get; }

        public CacheEntry(int endPosition, int maxPeek, object result)
        {
            EndPosition = endPosition;
            MaxPeek = maxPeek;
            Result = result;
        }
    }
}