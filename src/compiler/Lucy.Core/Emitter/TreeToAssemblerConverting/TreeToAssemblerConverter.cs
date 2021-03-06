using Lucy.Assembler;
using Lucy.Assembler.ContainerFormats.PE;
using Lucy.Core.Helper;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.Parsing;

namespace Lucy.Core.Compiler.TreeToAssemblerConverting
{
    public static class TreeToAssemblerConverter
    {
        public static void Run(SyntaxTreeNode node, WinExecutableEmitterContext ctx)
        {
            switch (node)
            {
                case FunctionDeclarationStatementSyntaxNode fd:
                    FunctionDeclarationToAssemblerConverter.Run(fd, ctx);
                    break;
                case FunctionCallExpressionSyntaxNode fc:
                    FunctionCallToAssemblerConverter.Run(fc, ctx);
                    break;
                case StringConstantExpressionSyntaxNode sc:
                    StringConstantToAssemblerConverter.Run(sc, ctx);
                    break;
                case NumberConstantExpressionSyntaxNode nc:
                    NumberConstantToAssemblerConverter.Run(nc, ctx);
                    break;
                default:
                    foreach (var child in node.GetChildNodes())
                        Run(child, ctx);
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
