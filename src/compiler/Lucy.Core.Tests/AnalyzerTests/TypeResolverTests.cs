using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Handler;
using Shouldly;

namespace Lucy.Core.Tests.AnalyzerTests;

public class TypeResolverTests : AnalyzerTestBase
{
    [Test]
    public void Should_resolve_the_type_of_a_typed_variable_declaration_correctly()
    {
        AddDoc("var num: int = 0");

        var type = Get<TypeResolver>().GetTypeInfo(Find<VariableDeclarationStatementSyntaxNode>("num").NodeId);

        type.ShouldNotBeNull();
        type.Name.ShouldBe("int");
        type.TypeDeclarationNodeId.ShouldBeNull();
    }
    
    [Test]
    public void Should_resolve_the_type_of_a_untyped_variable_declaration_correctly()
    {
        AddDoc("var num = 0");

        var type = Get<TypeResolver>().GetTypeInfo(Find<VariableDeclarationStatementSyntaxNode>("num").NodeId);

        type.ShouldNotBeNull();
        type.Name.ShouldBe("int");
        type.TypeDeclarationNodeId.ShouldBeNull();
    }
    
    [Test]
    public void Should_resolve_the_type_of_a_function_call_correctly()
    {
        AddDoc("""
        fun test(): int {}
        fun main(): void {
            test()
        }
        """);

        var funCall = Find<FunctionCallExpressionSyntaxNode>("test").NodeId;
        var type = Get<TypeResolver>().GetTypeInfo(funCall);

        type.ShouldNotBeNull();
        type.Name.ShouldBe("int");
        type.TypeDeclarationNodeId.ShouldBeNull();
    }
    
    [Test]
    public void Should_resolve_the_type_of_a_variable_correctly()
    {
        AddDoc("""
        fun main(): void {
            var t = 0
            t
        }
        """);

        var varRef = Find<VariableReferenceExpressionSyntaxNode>("t").NodeId;
        var type = Get<TypeResolver>().GetTypeInfo(varRef);

        type.ShouldNotBeNull();
        type.Name.ShouldBe("int");
        type.TypeDeclarationNodeId.ShouldBeNull();
    }
    
    [Test]
    public void Should_resolve_the_type_of_function_return_type_correctly()
    {
        AddDoc("""
        fun main(): void {
        }
        """);

        var varRef = Find<FunctionDeclarationStatementSyntaxNode>("main").ReturnType.TypeReference.NodeId;
        var type = Get<TypeResolver>().GetTypeInfo(varRef);

        type.ShouldNotBeNull();
        type.Name.ShouldBe("void");
        type.TypeDeclarationNodeId.ShouldBeNull();
    }
}