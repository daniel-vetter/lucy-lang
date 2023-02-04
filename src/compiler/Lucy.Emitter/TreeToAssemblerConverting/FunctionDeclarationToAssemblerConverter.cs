using Lucy.Core.SemanticAnalysis;
using Lucy.Core.Model;

namespace Lucy.Emitter.TreeToAssemblerConverting
{
    internal class FunctionDeclarationToAssemblerConverter
    {
        internal static void Run(FunctionDeclarationStatementSyntaxNode fd, SemanticAnalyzer semanticModel, WinExecutableEmitterContext ctx)
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

    public record EmitterFunctionId(string Guid)
    {
        public override string ToString() => $"FunctionId({Guid})";
    }
}