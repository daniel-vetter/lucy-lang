using System.Collections.Immutable;
using Lucy.Core.Model;
using Lucy.Core.Parsing;
using Shouldly;

namespace Lucy.Core.Tests
{
    internal class ParserTests
    {
        [Test]
        public void Test()
        {
            var code = """
                fun test1(): void {
                }

                fun test2(): void {
                }
                """;

            var p = new ParserResult("/doc", code);
            var v1 = p.RootNode;


            var pos = code.IndexOf("test2", StringComparison.Ordinal);
            p.Update(pos, 5, "test3");

            var v2 = p.RootNode;
            
            var recreatedNodes = Flatten(v2).Except(Flatten(v1)).ToArray();

            recreatedNodes.Length.ShouldBe(4);
        }

        ImmutableArray<SyntaxTreeNodeBuilder> Flatten(SyntaxTreeNodeBuilder node)
        {
            var r = ImmutableArray.CreateBuilder<SyntaxTreeNodeBuilder>();
            void Traverse(SyntaxTreeNodeBuilder parent)
            {
                r.Add(parent);
                foreach (var child in parent.GetChildNodes()) 
                    Traverse(child);
            }
            Traverse(node);
            return r.ToImmutable();
        }
    }
}
