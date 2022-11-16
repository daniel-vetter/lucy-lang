using Lucy.Core.ProjectManagement;
using Shouldly;

namespace Lucy.Core.Tests
{
    public class LineBreakMapTests
    {
        [Test]
        public void Test_conversion_from_1D_to_2D()
        {
            var map = new LineBreakMap("AB\nCD");

            map.ConvertTo2D(0).ShouldBe(new Position2D(0, 0));
            map.ConvertTo2D(1).ShouldBe(new Position2D(0, 1));
            map.ConvertTo2D(2).ShouldBe(new Position2D(0, 2));
            map.ConvertTo2D(3).ShouldBe(new Position2D(1, 0));
            map.ConvertTo2D(4).ShouldBe(new Position2D(1, 1));
            map.ConvertTo2D(5).ShouldBe(new Position2D(1, 2));
            map.ConvertTo2D(6).ShouldBe(new Position2D(1, 2));
            map.ConvertTo2D(7).ShouldBe(new Position2D(1, 2));
        }

        [Test]
        public void Test_conversion_from_2D_to_1D()
        {
            var map = new LineBreakMap("AB\nCD");

            map.ConvertTo1D(0, 0).ShouldBe(0);
            map.ConvertTo1D(0, 1).ShouldBe(1);
            map.ConvertTo1D(0, 2).ShouldBe(2);
            map.ConvertTo1D(1, 0).ShouldBe(3);
            map.ConvertTo1D(1, 1).ShouldBe(4);
            map.ConvertTo1D(1, 2).ShouldBe(5);
            map.ConvertTo1D(1, 3).ShouldBe(5);
            map.ConvertTo1D(1, 4).ShouldBe(5);
            map.ConvertTo1D(2, 0).ShouldBe(5);
        }
    }
}
