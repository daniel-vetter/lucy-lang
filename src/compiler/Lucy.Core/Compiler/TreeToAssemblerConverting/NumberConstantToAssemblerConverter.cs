﻿using Disassembler.Assembling;
using Disassembler.Assembling.Operations;
using Lucy.Core.Parser.Nodes.Expressions.Unary;

namespace Lucy.Core.Compiler.TreeToAssemblerConverting
{
    internal class NumberConstantToAssemblerConverter
    {
        internal static void Run(NumberConstantExpressionSyntaxNode nc, AsmConvertContext ctx)
        {
            ctx.Assembler.AddOperation(new Mov(Register.EAX, new Immediate(OperandSize.S32, (uint)nc.Value)));
        }
    }
}
