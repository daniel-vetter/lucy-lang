using Lucy.Assembler;
using Lucy.Assembler.ContainerFormats.PE;
using Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.SemanticAnalysis;
using System;

namespace Lucy.Core.Compiler.TreeToAssemblerConverting
{
    internal class FunctionDeclarationToAssemblerConverter
    {
        internal static void Run(FunctionDeclarationStatementSyntaxNode fd, WinExecutableEmitterContext ctx)
        {
            var info = fd.GetFunctionInfo();
            if (info == null)
                throw new Exception("No " + nameof(FunctionInfo) + " on function declaration found.");

            

            if (info.Extern != null)
            {
                ctx.ImportTable.Add(new ImportTableEntry(info.Extern.LibraryName, info.Extern.FunctionName));
                return;
            }



            var id = new EmitterFunctionId(Guid.NewGuid());
            fd.SetAnnotation(id);
            ctx.Assembler.AddSpacer();
            ctx.Assembler.AddLabel(id, "Function: " + info.Name);
            
            if (info.IsEntryPoint)
                ctx.Assembler.AddLabel(new AddressExport(new EntryPoint()));

            if (fd.Body != null)
                TreeToAssemblerConverter.Run(fd.Body, ctx);
        }
    }

    public record EmitterFunctionId(Guid guid)
    {
        public override string ToString() => guid.ToString();
    }
}