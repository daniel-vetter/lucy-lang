using System;
using System.Collections.Generic;

namespace Lucy.Core.ProjectManagement
{
    public class LineBreakMap
    {
        private List<int> _lineStart;
        private List<int> _lineLengths;

        public LineBreakMap(string content)
        {
            var lengths = new List<int>();
            var starts = new List<int>();
            var count = 0;
            var start = 0;
            for (int i=0;i<content.Length;i++)
            {
                count++;
                if (content[i] == '\n')
                {
                    lengths.Add(count);
                    starts.Add(start);
                    count = 0;
                    start = i + 1;
                }
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
                throw new ArgumentException("Invalid position: " + position.Position, nameof(position.Position));

            if (position.Position > _lineStart[^1] + _lineLengths[^1])
                return new Position2D(_lineStart.Count - 1, _lineLengths[^1]);

            var index = FindIndex(i =>
            {
                if (position.Position < _lineStart[i]) return 1;
                if (position.Position > _lineStart[i] + _lineLengths[i]) return -1;
                return 0;
            });

            if (index == -1)
                throw new Exception("Invalid position: " + position);

            return new Position2D(index, position.Position - _lineStart[index]);
        }

        public Position1D To1D(Position2D position)
        {
            if (position.Line < 0) throw new ArgumentException("Invalid line: " + position.Line, nameof(position));
            if (position.Character < 0) throw new ArgumentException("Invalid character: " + position.Character, nameof(position));

            if (position.Line >= _lineStart.Count)
                return new Position1D(_lineStart[^1] + _lineLengths[^1]);

            var c = position.Character;
            if (position.Character > _lineLengths[position.Line])
                c = _lineLengths[position.Line];

            return new Position1D(_lineStart[position.Line] + c);
        }

        private int FindIndex(Func<int, int> compare)
        {
            var min = 0;
            var max = _lineLengths.Count;
            var mid = ((max - min) / 2) + min;
            while (min < max)
            {
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

    public record Position1D(int Position);
    public record Range1D(Position1D Start, Position1D End);
    public record Position2D(int Line, int Character);
    public record Range2D(Position2D Start, Position2D End);
}
