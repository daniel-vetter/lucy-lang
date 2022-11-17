namespace Lucy.Core.ProjectManagement
{
    public class WorkspaceDocument
    {
        private LineBreakMap? _lineBreakMap;

        public WorkspaceDocument(string path, string content)
        {
            Path = path;
            Content = content;
        }

        public string Path { get; }
        public string Content { get; }

        public Range1D ConvertTo1D(Range2D range) => new Range1D(ConvertTo1D(range.Start), ConvertTo1D(range.End));
        public Position1D ConvertTo1D(Position2D position) 
        {
            _lineBreakMap ??= new LineBreakMap(Content);
            return _lineBreakMap.To1D(position);
        }

        public Range2D ConvertTo2D(Range1D range) => new Range2D(ConvertTo2D(range.Start), ConvertTo2D(range.End));
        public Position2D ConvertTo2D(Position1D position)
        {
            _lineBreakMap ??= new LineBreakMap(Content);
            return _lineBreakMap.To2D(position);
        }
    }
}
