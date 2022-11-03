using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Lucy.Core.Parsing.Nodes
{
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
        private Dictionary<TKey, TValue> _dict;

        public ComparableReadOnlyDictionary()
        {
            _dict = new Dictionary<TKey, TValue>();
        }

        public ComparableReadOnlyDictionary(IDictionary<TKey, TValue> entries)
        {
            _dict = new Dictionary<TKey, TValue>(entries);
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

            foreach (var (key, value) in _dict)
            {
                if (!other._dict.TryGetValue(key, out var otherValue))
                    return false;

                if (value == null ^ otherValue == null)
                    return false;

                if (value != null)
                    if (!value.Equals(otherValue))
                        return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hc = new HashCode();
            foreach (var key in _dict.Keys.OrderBy(x => x))
                hc.Add(key);
            foreach (var value in _dict.Values.OrderBy(x => x))
                hc.Add(value);
            return hc.ToHashCode();
        }

        public class Builder
        {
            Dictionary<TKey, TValue> _builderDict = new();

            public void Add(TKey key, TValue value)
            {
                _builderDict.Add(key, value);
            }

            public ComparableReadOnlyDictionary<TKey, TValue> Build()
            {
                var r = new ComparableReadOnlyDictionary<TKey, TValue>();
                r._dict = _builderDict;
                return r;
            }
        }

        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
    }
}
