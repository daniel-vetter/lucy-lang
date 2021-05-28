using Lucy.Assembler;
using Lucy.Assembler.ContainerFormats.PE;
using Lucy.Assembler.Operations;
using Lucy.Core.Parser.Nodes.Expressions.Unary;
using Lucy.Core.SemanticAnalysis;
using System;

namespace Lucy.Core.Compiler.TreeToAssemblerConverting
{
    internal class FunctionCallToAssemblerConverter
    {
        internal static void Run(FunctionCallExpressionSyntaxNode fc, WinExecutableEmitterContext ctx)
        {
            for (int i = fc.ArgumentList.Count - 1; i >= 0; i--)
            {
                var arg = fc.ArgumentList[i];
                TreeToAssemblerConverter.Run(arg, ctx);
                ctx.Assembler.AddOperation(new Push(Register.EAX));
            }

            var functionInfo = fc.GetFunctionInfo();
            if (functionInfo == null)
                throw new Exception("No function info found.");

            if (functionInfo.Extern == null)
                throw new Exception("Only extern functions are currently supported.");

            ctx.Assembler.AddOperation(new Call(new Memory(OperandSize.S32, 0, new AddressImport(new ImportAddressTableEntry(functionInfo.Extern.LibraryName, functionInfo.Extern.FunctionName), AddressType.AbsoluteVirtualAddress))));
        }
    }
}
