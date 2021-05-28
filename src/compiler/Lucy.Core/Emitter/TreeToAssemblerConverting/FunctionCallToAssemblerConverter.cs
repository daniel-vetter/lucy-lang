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

            Call? call = null;
            if (functionInfo.Extern == null)
            {
                //TODO: Should just use call rel32
                ctx.Assembler.AddOperation(new Mov(Register.EAX, new Immediate(OperandSize.S32, 0, new AddressImport(new EmitterFunctionId(functionInfo.Id), AddressType.AbsoluteVirtualAddress))));
                call = new Call(Register.EAX);
            }
            else
            {
                call = new Call(new Memory(OperandSize.S32, 0, new AddressImport(new ImportAddressTableEntry(functionInfo.Extern.LibraryName, functionInfo.Extern.FunctionName), AddressType.AbsoluteVirtualAddress)));
                
            }

            ctx.Assembler.AddOperation(call);
        }
    }
}
