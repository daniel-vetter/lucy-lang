using Lucy.Core.Parsing;
using Lucy.Core.ProjectManagement;

namespace Lucy.Core.Tests
{
    internal class ParserTests
    {
        [Test]
        public void Test()
        {
            var code = """
                fun main(): void {
                b
                }
                """;

            var result = Parser.Parse("/doc", code);
            Parser.Update(result, new Range1D(20, 21), "");
        }
    }
}
