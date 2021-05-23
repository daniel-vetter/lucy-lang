﻿using Disassembler.Assembling;
using Disassembler.Assembling.Operations;
using Disassembler.ContainerFormats.PE;
using Lucy.Core.Parser.Nodes.Expressions.Unary;
using Lucy.Core.SemanticAnalysis;
using System;

namespace Lucy.Core.Compiler.TreeToAssemblerConverting
{
    internal class FunctionCallToAssemblerConverter
    {
        internal static void Run(FunctionCallExpressionSyntaxNode fc, AsmConvertContext ctx)
        {
            for (int i = fc.ArgumentList.Count - 1; i >= 0; i--)
            {
                var arg = fc.ArgumentList[i];
                TreeToAssemblerConverter.Run(arg, ctx);
                ctx.Assembler.Add(new Push(Register.EAX));
            }

            var functionInfo = fc.GetFunctionInfo();
            if (functionInfo == null)
                throw new Exception("No function info found.");

            if (functionInfo.Extern == null)
                throw new Exception("Only extern functions are currently supported.");

            ctx.Assembler.Add(new Call(new Memory(OperandSize.S32, 0, new AddressImport(new ImportAddressTableEntry(functionInfo.Extern.LibraryName, functionInfo.Extern.FunctionName), AddressType.AbsoluteVirtualAddress))));
        }
    }
}