using System;
using System.Collections.Generic;
using System.Globalization;

namespace Lucy.Core.ProjectManagement;

public class LineBreakMap
{
    private readonly List<int> _lineStart;
    private readonly List<int> _lineLengths;

    public static LineBreakMap CreateFrom(string content)
    {
        return new LineBreakMap(content);
    }

    private LineBreakMap(string content)
    {
        var lengths = new List<int>();
        var starts = new List<int>();
        var count = 0;
        var start = 0;
        for (var i = 0; i < content.Length; i++)
        {
            count++;
            if (content[i] != '\n') 
                continue;
            lengths.Add(count);
            starts.Add(start);
            count = 0;
            start = i + 1;
        }
        if (count != 0)
        {
            lengths.Add(count);
            starts.Add(start);
        }

        _lineLengths = lengths;
        _lineStart = starts;
    }

    public Position2D To2D(Position1D position)
    {
        if (position.Position < 0)
            throw new ArgumentException($"Invalid position: {position.Position}", nameof(position.Position));

        if (_lineStart.Count == 0)
            return new Position2D(0, 0);

        if (position.Position > _lineStart[^1] + _lineLengths[^1])
            return new Position2D(_lineStart.Count - 1, _lineLengths[^1]);

        var index = FindIndex(i =>
        {
            if (position.Position < _lineStart[i]) return 1;
            if (position.Position > _lineStart[i] + _lineLengths[i]) return -1;
            return 0;
        });

        if (index == -1)
            throw new Exception($"Invalid position: {position}");

        return new Position2D(index, position.Position - _lineStart[index]);
    }

    public Range2D To2D(Range1D range) => new Range2D(To2D(range.Start), To2D(range.End));

    public Position1D To1D(Position2D position)
    {
        if (position.Line < 0) throw new ArgumentException($"Invalid line: {position.Line}", nameof(position));
        if (position.Character < 0) throw new ArgumentException($"Invalid character: {position.Character}", nameof(position));

        if (_lineStart.Count == 0)
            return new Position1D(0);

        if (position.Line >= _lineStart.Count)
            return new Position1D(_lineStart[^1] + _lineLengths[^1]);

        var c = position.Character;
        if (position.Character > _lineLengths[position.Line])
            c = _lineLengths[position.Line];

        return new Position1D(_lineStart[position.Line] + c);
    }

    public Range1D To1D(Range2D range) => new Range1D(To1D(range.Start), To1D(range.End));

    private int FindIndex(Func<int, int> compare)
    {
        var min = 0;
        var max = _lineLengths.Count;

        while (min < max)
        {
            var mid = ((max - min) / 2) + min;
            var result = compare(mid);
            if (result < 0)
                min = mid + 1;
            else if (result > 0)
                max = mid - 1;
            else return mid;
        }
        if (min == max && min < _lineLengths.Count && compare(min) == 0)
            return min;

        return -1;
    }
}

public readonly struct Position1D : IEquatable<Position1D>
{
    public Position1D(int position) => Position = position;

    public int Position { get; }
    public override string ToString()
    {
        return Position.ToString(CultureInfo.InvariantCulture);
    }

    public override bool Equals(object? obj) => obj is Position1D d && Equals(d);
    public bool Equals(Position1D other) => Position == other.Position;
    public override int GetHashCode() => HashCode.Combine(Position);
    public static bool operator ==(Position1D left, Position1D right) => left.Equals(right);
    public static bool operator !=(Position1D left, Position1D right) => !(left == right);
}

public readonly struct Range1D : IEquatable<Range1D>
{
    public Range1D(Position1D start, Position1D end)
    {
        Start = start;
        End = end;
    }

    public Range1D(int start, int end)
    {
        Start = new Position1D(start);
        End = new Position1D(end);
    }

    public bool IntersectsWith(Range1D range)
    {
        return range.Start.Position < End.Position && Start.Position < range.End.Position;
    }

    public override string ToString()
    {
        return $"{Start}-{End}";
    }

    public Position1D Start { get; }
    public Position1D End { get; }
    public int Length => End.Position - Start.Position;

    public override bool Equals(object? obj) => obj is Range1D d && Equals(d);
    public bool Equals(Range1D other) => Start.Equals(other.Start) && End.Equals(other.End);
    public override int GetHashCode() => HashCode.Combine(Start, End);
    public static bool operator ==(Range1D left, Range1D right) => left.Equals(right);
    public static bool operator !=(Range1D left, Range1D right) => !(left == right);
}

public readonly struct Position2D : IEquatable<Position2D>
{
    public Position2D(int line, int character)
    {
        Line = line;
        Character = character;
    }

    public int Line { get; }
    public int Character { get; }

    public override bool Equals(object? obj) => obj is Position2D d && Equals(d);
    public bool Equals(Position2D other) => Line == other.Line && Character == other.Character;
    public override int GetHashCode() => HashCode.Combine(Line, Character);
    public override string ToString() => $"[{Line + 1}, {Character + 1}]";
    public static bool operator ==(Position2D left, Position2D right) => left.Equals(right);
    public static bool operator !=(Position2D left, Position2D right) => !(left == right);
}

public readonly struct Range2D : IEquatable<Range2D>
{
    public Range2D(Position2D start, Position2D end)
    {
        Start = start;
        End = end;
    }

    public Position2D Start { get; }
    public Position2D End { get; }

    public override bool Equals(object? obj) => obj is Range2D d && Equals(d);
    public bool Equals(Range2D other) => Start.Equals(other.Start) && End.Equals(other.End);
    public override int GetHashCode() => HashCode.Combine(Start, End);
    public static bool operator ==(Range2D left, Range2D right) => left.Equals(right);
    public static bool operator !=(Range2D left, Range2D right) => !(left == right);
}