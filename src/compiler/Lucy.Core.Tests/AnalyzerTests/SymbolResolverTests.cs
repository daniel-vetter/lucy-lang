using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Handler;
using Shouldly;

namespace Lucy.Core.Tests.AnalyzerTests;

public class SymbolResolverTests : AnalyzerTestBase
{
    [Test]
    public void Should_resolve_functions_in_the_same_file_correctly()
    {
        AddDoc("""
        fun test(): void {}
        fun main(): void { test() }
        """);
        
        var functionCallSymbolId = Find<FunctionCallExpressionSyntaxNode>("test").FunctionName.NodeId;

        var expected = Find<FunctionDeclarationStatementSyntaxNode>("test");
        var foundList = Get<SymbolResolver>().GetSymbolDeclarations(functionCallSymbolId);
        
        foundList.Count.ShouldBe(1);
        foundList[0].DeclaringNodeId.ShouldBe(expected.NodeId);
    }
    
    [Test]
    public void Should_resolve_functions_in_a_imported_file_correctly()
    {
        AddDoc("/file1.lucy", """
        import "file2"
        fun main(): void { test() }
        """);
        
        AddDoc("/file2.lucy", "fun test(): void {}");

        var functionCallSymbolId = Find<FunctionCallExpressionSyntaxNode>("test").FunctionName.NodeId;
        
        var expected = Find<FunctionDeclarationStatementSyntaxNode>(x => x.FunctionName.Text == "test");
        var foundList = Get<SymbolResolver>().GetSymbolDeclarations(functionCallSymbolId);
        
        foundList.Count.ShouldBe(1);
        foundList[0].DeclaringNodeId.ShouldBe(expected.NodeId);
    }
}