using Lucy.Assembler;
using Lucy.Assembler.ContainerFormats.PE;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.SemanticAnalysis;
using System;

namespace Lucy.Emitter.TreeToAssemblerConverting
{
    internal class FunctionDeclarationToAssemblerConverter
    {
        internal static void Run(FunctionDeclarationStatementSyntaxNode fd, SemanticDatabase semanticModel, WinExecutableEmitterContext ctx)
        {
            /*
            var info = semanticModel.GetFunctionInfo(fd);
            
            if (info.Extern != null)
            {
                ctx.ImportTable.Add(new ImportTableEntry(info.Extern.LibraryName, info.Extern.FunctionName));
                return;
            }

            ctx.Assembler.AddSpacer();
            ctx.Assembler.AddLabel(new EmitterFunctionId(info.Id), "Function: " + info.Name);
            
            if (info.IsEntryPoint)
                ctx.Assembler.AddLabel(new EntryPointAnnotation());

            if (fd.Body != null)
                TreeToAssemblerConverter.Run(fd.Body, semanticModel, ctx);
            */
        }
    }

    public record EmitterFunctionId(string guid)
    {
        public override string ToString() => $"FunctionId({guid})";
    }
}