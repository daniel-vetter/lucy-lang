using Lucy.Assembler;
using Lucy.Assembler.ContainerFormats.PE;
using Lucy.Core.Helper;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Expressions.Unary;
using Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration;

namespace Lucy.Core.Compiler.TreeToAssemblerConverting
{
    public static class TreeToAssemblerConverter
    {
        public static void Run(SyntaxNode node, AsmConvertContext ctx)
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
                        Run(child.Node, ctx);
                    break;
            }
        }
    }

    public class AsmConvertContext
    {
        public AsmConvertContext(Lucy.Assembler.AssemblyBuilder assembler, ImportTableSection importTable, DataSection data)
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
