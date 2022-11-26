using System.Collections.Generic;
using System;
using System.Collections;

namespace Lucy.Core.Parsing.Nodes;

public static class ComparableReadOnlyListExtensionMethods
{
    public static ComparableReadOnlyList<T> ToComparableReadOnlyList<T>(this IEnumerable<T> list)
    {
        return new ComparableReadOnlyList<T>(list);
    }
}

public class ComparableReadOnlyList<T> : IEnumerable<T>, IEquatable<ComparableReadOnlyList<T>>
{
    private readonly List<T> _list;
    private readonly int _hash;
    
    public ComparableReadOnlyList()
    {
        _list = new List<T>();
        _hash = 0;
    }

    public ComparableReadOnlyList(IEnumerable<T> collection)
    {
        _list = new List<T>(collection);
        _hash = ComputeHash();
    }

    private ComparableReadOnlyList(List<T> collection)
    {
        _list = collection;
        _hash = ComputeHash();
    }

    private int ComputeHash()
    {
        var h = new HashCode();
        foreach (var entry in _list)
            h.Add(entry);
        return h.ToHashCode();
    }

    public int Count => _list.Count;
    public T this[int index] => _list[index];

    public override int GetHashCode() => _hash;

    public override bool Equals(object? obj)
    {
        return Equals(obj as ComparableReadOnlyList<T>);
    }

    public bool Equals(ComparableReadOnlyList<T>? other)
    {
        if (other == null)
            return false;

        if (other.Count != Count)
            return false;

        for (var i = 0; i < Count; i++)
        {
            var left = other[i];
            var right = this[i];
            if (left == null && right != null) return false;
            if (left != null && !left.Equals(right))
                return false;
        }
        return true;
    }

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();
    
    public class Builder
    {
        private readonly List<T> _builderList = new();
        private bool _done;

        public void Add(T value)
        {
            if (_done)
                throw new Exception("List was already build.");

            _builderList.Add(value);
        }

        public void AddRange(IEnumerable<T> values)
        {
            if (_done)
                throw new Exception("List was already build.");

            _builderList.AddRange(values);
        }

        public ComparableReadOnlyList<T> Build()
        {
            var result = new ComparableReadOnlyList<T>(_builderList);
            _done = true;
            return result;
        }
    }
}