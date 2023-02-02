using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Handler;
using Shouldly;

namespace Lucy.Core.Tests.AnalyzerTests;

public class RangeResolverTests : AnalyzerTestBase
{
    [Test]
    public void Should_find_the_correct_node_based_on_a_position()
    {
        AddDoc("fun te#st() {}");

        var node = Resolve(Get<RangeResolver>().GetNodeIdAtPosition(CursorPath, CursorPosition));

        var tokenNode = node.ShouldBeOfType<TokenNode>();
        tokenNode.Text.ShouldBe("test");
    }
    
    [Test]
    public void Should_find_the_correct_range_based_on_a_node_id()
    {
        AddDoc("fun test() {}");

        var range = Get<RangeResolver>().GetTrimmedRangeFromNodeId(FindId("test"));
        
        range.Start.Position.ShouldBe(4);
        range.End.Position.ShouldBe(8);
    }
}