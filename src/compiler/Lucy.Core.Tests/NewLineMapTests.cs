using Lucy.Core.ProjectManagement;
using Shouldly;

namespace Lucy.Core.Tests;

public class LineBreakMapTests
{
    [Test]
    public void Test_conversion_from_1D_to_2D()
    {
        var map = new LineBreakMap("AB\nCD");

        map.To2D(new Position1D(0)).ShouldBe(new Position2D(0, 0));
        map.To2D(new Position1D(1)).ShouldBe(new Position2D(0, 1));
        map.To2D(new Position1D(2)).ShouldBe(new Position2D(0, 2));
        map.To2D(new Position1D(3)).ShouldBe(new Position2D(1, 0));
        map.To2D(new Position1D(4)).ShouldBe(new Position2D(1, 1));
        map.To2D(new Position1D(5)).ShouldBe(new Position2D(1, 2));
        map.To2D(new Position1D(6)).ShouldBe(new Position2D(1, 2));
        map.To2D(new Position1D(7)).ShouldBe(new Position2D(1, 2));
    }

    [Test]
    public void Test_empty_strings_work()
    {
        var map = new LineBreakMap("");
        map.To2D(new Position1D(0)).ShouldBe(new Position2D(0, 0));
        map.To2D(new Position1D(1)).ShouldBe(new Position2D(0, 0));

        map.To1D(new Position2D(0, 0)).ShouldBe(new Position1D(0));
        map.To1D(new Position2D(0, 1)).ShouldBe(new Position1D(0));
        map.To1D(new Position2D(1, 0)).ShouldBe(new Position1D(0));

    }

    [Test]
    public void Test_conversion_from_2D_to_1D()
    {
        var map = new LineBreakMap("AB\nCD");

        map.To1D(new Position2D(0, 0)).ShouldBe(new Position1D(0));
        map.To1D(new Position2D(0, 1)).ShouldBe(new Position1D(1));
        map.To1D(new Position2D(0, 2)).ShouldBe(new Position1D(2));
        map.To1D(new Position2D(1, 0)).ShouldBe(new Position1D(3));
        map.To1D(new Position2D(1, 1)).ShouldBe(new Position1D(4));
        map.To1D(new Position2D(1, 2)).ShouldBe(new Position1D(5));
        map.To1D(new Position2D(1, 3)).ShouldBe(new Position1D(5));
        map.To1D(new Position2D(1, 4)).ShouldBe(new Position1D(5));
        map.To1D(new Position2D(2, 0)).ShouldBe(new Position1D(5));
    }
}