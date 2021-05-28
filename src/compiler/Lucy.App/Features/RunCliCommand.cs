using Lucy.App.Infrastructure.Cli;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Compiler;
using Lucy.Core.ProjectManagement;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace Lucy.App.Features
{
    [Service]
    class RunCliCommand : ICliCommand
    {
        public void Register(CommandLineBuilder builder)
        {
            var cmd = new Command("run", "Runs a lucy script");
            cmd.Handler = CommandHandler.Create(Run);
            builder.AddCommand(cmd);
        }

        private async Task<int> Run()
        {
            var workspace = await Workspace.CreateFromPath(Environment.CurrentDirectory);
            workspace.Process();

            var tempFileName = Path.ChangeExtension(Path.GetTempFileName(), "exe");
            tempFileName = "C:\\temp\\out.exe";
            Console.WriteLine(WinExecutableEmitter.GetAssemblyCode(workspace));
            await WinExecutableEmitter.Emit(workspace, tempFileName);

            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = tempFileName;
            p.Start();
            await p.WaitForExitAsync();
            File.Delete(tempFileName);
            return p.ExitCode;
        }
    }
}
