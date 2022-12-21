using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes;

public static class ComparableReadOnlyDictionaryExtensionMethods
{
    public static ComparableReadOnlyDictionary<TKey, TValue> ToComparableReadOnlyDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector) where TKey : notnull
    {
        var b = new ComparableReadOnlyDictionary<TKey, TValue>.Builder();
        foreach (var entry in source)
            b.Add(keySelector(entry), valueSelector(entry));
        return b.Build();
    }
}

public class ComparableReadOnlyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IEquatable<ComparableReadOnlyDictionary<TKey, TValue>> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dict;
    private readonly List<KeyValuePair<TKey, TValue>> _stableList;
    private readonly int _hash;

    private ComparableReadOnlyDictionary(Dictionary<TKey, TValue> dictionary, List<KeyValuePair<TKey, TValue>> stableList)
    {
        _dict = dictionary;
        _stableList = stableList;
        _hash = ComputeHash();
    }

    public int Count => _dict.Count;

    public IEnumerable<TKey> Keys => _dict.Keys;

    public IEnumerable<TValue> Values => _dict.Values;

    public TValue this[TKey index] => _dict[index];

    public override bool Equals(object? obj)
    {
        return Equals(obj as ComparableReadOnlyDictionary<TKey, TValue>);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_dict.TryGetValue(key, out var outValue))
        {
            value = outValue;
            return true;
        }

        value = default;
        return false;
    }

    public bool Equals(ComparableReadOnlyDictionary<TKey, TValue>? other)
    {
        if (other == null)
            return false;

        if (Count != other.Count)
            return false;

        for(var i=0;i<_stableList.Count;i++)
        {
            if (_stableList[i].GetHashCode() != other._stableList[i].GetHashCode())
                return false;

            if (!_stableList[i].Equals(other._stableList[i]))
                return false;
        }

        return true;
    }

    public override int GetHashCode() => _hash;

    private int ComputeHash()
    {
        var hc = new HashCode();
        foreach(var (key, value) in _stableList)
        {
            hc.Add(key);
            hc.Add(value);
        }
        return hc.ToHashCode();
    }

    public class Builder
    {
        private readonly Dictionary<TKey, TValue> _builderDict = new();
        private readonly List<KeyValuePair<TKey,TValue>> _stableList = new();

        public void Add(TKey key, TValue value)
        {
            _builderDict.Add(key, value);
            _stableList.Add(KeyValuePair.Create(key, value));
        }

        public ComparableReadOnlyDictionary<TKey, TValue> Build() => new(_builderDict, _stableList);
    }

    public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
}