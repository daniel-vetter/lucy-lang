using System;
using System.Collections.Generic;

namespace Lucy.Core.ProjectManagement
{
    public class LineBreakMap
    {
        private int[] _lineStart;
        private int[] _lineLengths;

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

            _lineLengths = lengths.ToArray();
            _lineStart = starts.ToArray();
        }

        public Position2D ConvertTo2D(int position)
        {
            if (position < 0)
                throw new ArgumentException("Invalid position: " + position, nameof(position));

            if (position > _lineStart[^1] + _lineLengths[^1])
                return new Position2D(_lineStart.Length - 1, _lineLengths[^1]);

            var index = FindIndex(i =>
            {
                if (position < _lineStart[i]) return 1;
                if (position > _lineStart[i] + _lineLengths[i]) return -1;
                return 0;
            });

            if (index == -1)
                throw new Exception("Invalid position: " + position);

            return new Position2D(index, position - _lineStart[index]);
        }

        public int ConvertTo1D(Position2D position)
        {
            return ConvertTo1D(position.Line, position.Character);
        }

        public int ConvertTo1D(int line, int character)
        {
            if (line < 0) throw new ArgumentException("Invalid line: " + line, nameof(line));
            if (character < 0) throw new ArgumentException("Invalid character: " + character, nameof(character));

            if (line >= _lineStart.Length)
                return _lineStart[^1] + _lineLengths[^1];

            if (character > _lineLengths[line])
                character = _lineLengths[line];

            return _lineStart[line] + character;
        }

        private int FindIndex(Func<int, int> compare)
        {
            var min = 0;
            var max = _lineLengths.Length;
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
            if (min == max && min < _lineLengths.Length && compare(min) == 0)
                return min;

            return -1;
        }
    }

    public record Position2D(int Line, int Character);
}
