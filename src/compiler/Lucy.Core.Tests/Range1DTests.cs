using Lucy.Core.ProjectManagement;
using Shouldly;

namespace Lucy.Core.Tests
{
    public class Range1DTests
    {
        [Test]
        public void IntersectionTests()
        {
            var r = new Range1D(10, 20);

            r.IntersectsWith(new Range1D(1,5)).ShouldBe(false);
            r.IntersectsWith(new Range1D(1, 10)).ShouldBe(false);
            r.IntersectsWith(new Range1D(5, 11)).ShouldBe(true);
            r.IntersectsWith(new Range1D(5, 15)).ShouldBe(true);
            r.IntersectsWith(new Range1D(10, 20)).ShouldBe(true);
            r.IntersectsWith(new Range1D(11, 19)).ShouldBe(true);
            r.IntersectsWith(new Range1D(15, 25)).ShouldBe(true);
            r.IntersectsWith(new Range1D(19, 25)).ShouldBe(true);
            r.IntersectsWith(new Range1D(20, 25)).ShouldBe(false);
            r.IntersectsWith(new Range1D(23, 29)).ShouldBe(false);
        }
    }
}
