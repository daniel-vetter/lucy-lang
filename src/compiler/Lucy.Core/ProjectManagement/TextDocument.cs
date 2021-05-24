using Lucy.Core.Model;

namespace Lucy.Core.ProjectManagement
{
    public class TextDocument
    {
        private string _content = "";
        private int _version = 1;
        private string _path;

        public TextDocument(string path, string content)
        {
            _path = path;
            _content = content;
        }

        private TextDocument(string path, string content, int version)
        {
            _path = path;
            _content = content;
            _version = version;
        }

        public int Version => _version;
        public string Path => _path;
        public string Content => _content;

        public TextDocument Change(Range1D range, string content)
        {
            var position = range.Position;
            var length = range.Length;

            if (position < 0) position = 0;
            if (position > _content.Length) position = _content.Length;
            if (position + length > _content.Length) length = _content.Length - position;

            var before = _content.Substring(0, position);
            var after = _content.Substring(position + length);
            return new TextDocument(_path, before + content + after, _version + 1);
        }

        public TextDocument Change(Range2D range, string content)
        {
            return Change(ConvertRange(range), content);
        }

        public Range1D ConvertRange(Range2D range)
        {
            var start = ConvertPosition(range.Start);
            var end = ConvertPosition(range.End);
            return new Range1D(start, end - start);
        }

        public Range2D ConvertRange(Range1D range)
        {
            return new Range2D(ConvertPosition(range.Position), ConvertPosition(range.Position + range.Length));
        }

        public Position2D ConvertPosition(int position)
        {
            int line = 0;
            int column = 0;
            if (position > _content.Length)
                position = _content.Length;
            for (int i = 0; i < position; i++)
            {
                if (_content[i] == '\n')
                {
                    column = 0;
                    line++;
                }
                else
                {
                    column++;
                }
            }
            return new Position2D(line, column);
        }

        public int ConvertPosition(Position2D position)
        {
            //Maybe some performance optimization?
            int line = 0;
            int column = 0;
            for (int i = 0; i < _content.Length; i++)
            {
                if (line == position.Line && column == position.Column)
                    return i;

                if (_content[i] == '\n')
                {
                    column = 0;
                    line++;
                }
                else
                {
                    column++;
                }
            }

            return _content.Length;
        }

        public override string ToString()
        {
            return _content;
        }
    }
}
