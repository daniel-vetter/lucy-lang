using Lucy.App.Infrastructure.Cli;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Compiler;
using Lucy.Core.ProjectManagement;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace Lucy.App.Features
{
    [Service(Lifetime.Singleton)]
    class CompileCliCommand : ICliCommand
    {
        public void Register(CommandLineBuilder builder)
        {
            var cmd = new Command("compile", "Compiles a lucy script");
            cmd.AddArgument(new Argument<FileInfo>("input-file", "The main file to compile."));
            cmd.Handler = CommandHandler.Create<FileInfo>(Run);
            builder.AddCommand(cmd);
        }

        private async Task Run(FileInfo? inputFile)
        {
            if (inputFile == null)
                throw new CliException("No input file was provided.");

            if (!inputFile.Exists)
                throw new CliException($"Could not find input file \"{inputFile}\".");

            if (inputFile.DirectoryName == null)
                throw new CliException($"Could not determin directory of input file \"{inputFile}\".");

            var workspace = await Workspace.CreateFromPath(inputFile.DirectoryName);
            
            var mainFile = workspace.Get("/" + inputFile.Name);
            if (mainFile == null)
                throw new CliException($"Could not find main script /\"{inputFile.DirectoryName}\" in parsed workspace.");

            await WinExecutableEmitter.Emit(workspace, "out.exe");
        }
    }
}
