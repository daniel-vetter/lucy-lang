namespace Lucy.Core.Model
{
    public record Range1D(int Position, int Length)
    {
        public static Range1D Combine(Range1D start, Range1D end)
        {
            return new Range1D(start.Position, end.Position + end.Length - start.Position);
        }
    }

    public record Range2D(Position2D Start, Position2D End);
    public record Position2D(int Line, int Column);

}
