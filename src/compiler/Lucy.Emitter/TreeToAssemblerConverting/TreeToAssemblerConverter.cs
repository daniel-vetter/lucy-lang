using Lucy.Assembler;
using Lucy.Assembler.ContainerFormats.PE;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.Parsing;
using Lucy.Core.SemanticAnalysis;

namespace Lucy.Emitter.TreeToAssemblerConverting
{
    public static class TreeToAssemblerConverter
    {
        public static void Run(SyntaxTreeNode node, SemanticAnalyzer semanticAnalyzer, WinExecutableEmitterContext ctx)
        {
            switch (node)
            {
                case FunctionDeclarationStatementSyntaxNode fd:
                    FunctionDeclarationToAssemblerConverter.Run(fd, semanticAnalyzer, ctx);
                    break;
                case FunctionCallExpressionSyntaxNode fc:
                    FunctionCallToAssemblerConverter.Run(fc, semanticAnalyzer, ctx);
                    break;
                case StringConstantExpressionSyntaxNode sc:
                    StringConstantToAssemblerConverter.Run(sc, ctx);
                    break;
                case NumberConstantExpressionSyntaxNode nc:
                    NumberConstantToAssemblerConverter.Run(nc, ctx);
                    break;
                default:
                    foreach (var child in node.GetChildNodes())
                        Run(child, semanticAnalyzer, ctx);
                    break;
            }
        }
    }

    public class WinExecutableEmitterContext
    {
        public WinExecutableEmitterContext(AssemblyBuilder assembler, ImportTableSection importTable, DataSection data)
        {
            Assembler = assembler;
            ImportTable = importTable;
            Data = data;
        }

        public AssemblyBuilder Assembler { get; }
        public ImportTableSection ImportTable { get; }
        public DataSection Data { get; }
    }
}
