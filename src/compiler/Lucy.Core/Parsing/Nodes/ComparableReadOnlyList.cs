using System.Collections.Generic;
using System;
using System.Collections;
using System.Xml.Linq;

namespace Lucy.Core.Parsing.Nodes;

public static class ComparableReadOnlyListExtensionMethods
{
    public static ComparableReadOnlyList<T> ToComparableReadOnlyList<T>(this IEnumerable<T> list)
    {
        return new ComparableReadOnlyList<T>(list);
    }
}

public abstract class ComparableReadOnlyList
{
    public static ComparableReadOnlyList<TElement> Create<TElement>(params TElement[] elements)
    {
        var l = new ComparableReadOnlyList<TElement>.Builder();
        foreach (var element in elements)
            l.Add(element);
        return l.Build();
    }
}

public class ComparableReadOnlyList<T> : IEnumerable<T>, IEquatable<ComparableReadOnlyList<T>>
{
    private List<T> _list;
    private int _hash;

    public class Builder
    {
        private List<T> _builderList = new();
        private bool _done = false;

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
            var list = new ComparableReadOnlyList<T>();
            list._list = _builderList;
            _done = true;
            list.ComputeHash();
            return list;
        }
    }

    public ComparableReadOnlyList()
    {
        _list = new List<T>();
        _hash = 0;
    }

    public ComparableReadOnlyList(IEnumerable<T> collection)
    {
        _list = new List<T>(collection);
        ComputeHash();
    }

    private void ComputeHash()
    {
        var h = new HashCode();
        for (int i = 0; i < _list.Count; i++)
        {
            h.Add(_list[i]);
        }
        _hash = h.ToHashCode();
    }

    public int Count => _list.Count;
    public T this[int index] => _list[index];

    public override int GetHashCode()
    {
        return _hash;
    }

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

        for (int i = 0; i < Count; i++)
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
}