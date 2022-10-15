using Lucy.Assembler;
using Lucy.Assembler.ContainerFormats.PE;
using Lucy.Core.Compiler.TreeToAssemblerConverting;
using Lucy.Core.ProjectManagement;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lucy.Core.Compiler
{
    public class WinExecutableEmitter
    {
        public static async Task Emit(Workspace workspace, string outFile)
        {
            var ctx = CreateAndProcess(workspace);

            var asmResult = ctx.Assembler.Process();
            if (asmResult.Issues.Any())
                throw new Exception($"Assembler reported issues: {Environment.NewLine}{string.Join(Environment.NewLine, asmResult.Issues.Select(x => x.Severity + ": " + x.Message))}");

            Console.WriteLine(ctx.Assembler.CreateAssemblerCode());

            var peBuilder = new PortableExecutableBuilder();
            peBuilder.AddSection(ctx.Data);
            peBuilder.AddSection(ctx.ImportTable);
            peBuilder.AddSection(new CodeSection(asmResult.Data));
            await peBuilder.Write(outFile);
        }

        public static string GetAssemblyCode(Workspace workspace)
        {
            return CreateAndProcess(workspace).Assembler.CreateAssemblerCode();
        }

        private static WinExecutableEmitterContext CreateAndProcess(Workspace workspace)
        {
            var ctx = new WinExecutableEmitterContext(
                new AssemblyBuilder(OperandSize.S32),
                new ImportTableSection(),
                new DataSection()
            );

            foreach (var doc in workspace.Documents)
            {
                if (doc.SyntaxTree == null)
                    throw new Exception($"Could not find a syntax tree for workspace document '{doc.Path}'.");

                if (doc.SemanticModel == null)
                    throw new Exception($"Could not find a sementic model for workspace document '{doc.Path}'.");

                TreeToAssemblerConverter.Run(doc.SyntaxTree, doc.SemanticModel, ctx);
            }

            return ctx;
        }
    }
}
