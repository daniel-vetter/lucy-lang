using Lucy.Assembler;
using Lucy.Assembler.Operations;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;

namespace Lucy.Emitter.TreeToAssemblerConverting
{
    internal class NumberConstantToAssemblerConverter
    {
        internal static void Run(NumberConstantExpressionSyntaxNode nc, WinExecutableEmitterContext ctx)
        {
            ctx.Assembler.AddOperation(new Mov(Register.EAX, new Immediate(OperandSize.S32, (uint)nc.Value)));
        }
    }
}
