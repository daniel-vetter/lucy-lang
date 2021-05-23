using Disassembler.ContainerFormats.PE;
using Lucy.Core.Helper;
using Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.SemanticAnalysis;
using System;

namespace Lucy.Core.Compiler.TreeToAssemblerConverting
{
    internal class FunctionDeclarationToAssemblerConverter
    {
        internal static void Run(FunctionDeclarationStatementSyntaxNode fd, AsmConvertContext ctx)
        {
            var info = fd.GetFunctionInfo();
            if (info == null)
                throw new Exception("No " + nameof(FunctionInfo) + " on function declaration found.");

            if (info.Extern != null)
            {
                ctx.ImportTable.Add(new ImportTableEntry(info.Extern.LibraryName, info.Extern.FunctionName));
            }

            foreach (var child in fd.GetChildNodes())
                TreeToAssemblerConverter.Run(child.Node, ctx);
        }
    }
}