using Lucy.Core.Model;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;
using System.Text;

namespace Lucy.Core.Tests
{
    public class Class1
    {
        [Test]
        public void Test()
        {
            var ws = new Workspace();
            ws.AddDocument("/main.lucy", """
                fun main() {

                }

                fun main2() {
                
                }

                fun main3() {
                
                }
                """);

            var sb = new StringBuilder();
            void Traverse(SyntaxTreeNode node)
            {
                if (node is DocumentRootSyntaxNode rootNode)
                    sb.Append(rootNode.LeadingTrivia);
                if (node is TokenNode t)
                {
                    sb.Append(t.Text);
                    sb.Append(t.TrailingTrivia);
                }
                foreach (var c in node.GetChildNodes())
                    Traverse(c);
            }

            Traverse((ws.GetFile("/main.lucy") as CodeWorkspaceDocument).ParserResult.RootNode);

            var t = sb.ToString();

            var sd = new SemanticDatabase(ws);

            var tokenNodeIds = sd.GetNodeIdsByType<TokenNode>("/main.lucy");
            var tokenNodes = tokenNodeIds.Select(x => sd.GetNodeById(x)).ToArray();

            var tokenNode = tokenNodes.Where(x => x.Text == "main").Single();

            var range = sd.GetRangeFromNode(tokenNode);

            var errors1 = sd.GetAllErrors();


            ws.UpdateFile("/main.lucy", new Range1D(new Position1D(31), new Position1D(31)), "num: int");
            var errors2 = sd.GetAllErrors();


            sb.Clear();
            Traverse((ws.GetFile("/main.lucy") as CodeWorkspaceDocument).ParserResult.RootNode);
            var gg = sb.ToString();
        }
    }
}
