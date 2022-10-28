using Lucy.Assembler;
using Lucy.Assembler.ContainerFormats.PE;
using Lucy.Assembler.Operations;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.SemanticAnalysis;
using System;

namespace Lucy.Core.Compiler.TreeToAssemblerConverting
{
    internal class FunctionCallToAssemblerConverter
    {
        internal static void Run(FunctionCallExpressionSyntaxNode fc, SemanticAnalyzer semanticModel, WinExecutableEmitterContext ctx)
        {
            /*
            var functionInfo = semanticModel.GetFunctionInfo(fc);
            if (functionInfo == null)
                throw new Exception("No function info found.");

            if (functionInfo.CallingConvention == CallingConvention.Cdecl)
            {
                for (int i = fc.ArgumentList.Count - 1; i >= 0; i--)
                {
                    TreeToAssemblerConverter.Run(fc.ArgumentList[i].Expression, semanticModel, ctx);
                    ctx.Assembler.AddOperation(new Push(Register.EAX));
                }
            }
            else
                throw new NotImplementedException("Missing callling convetion implementation");

            Call? call;
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

            if (functionInfo.CallingConvention == CallingConvention.Cdecl)
            {
                ctx.Assembler.AddOperation(new Add(Register.ESP, new Immediate(OperandSize.S32, (uint)(fc.ArgumentList.Count * 4))));
            }
            
            */
        }
    }
}
