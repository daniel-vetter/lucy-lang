using Lucy.App.Infrastructure.Cli;
using Lucy.App.Infrastructure.Output;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Compiler;
using Lucy.Core.Model;
using Lucy.Core.ProjectManagement;
using System;
using System.Collections.Generic;
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
        private readonly IOutput _output;

        public RunCliCommand(IOutput output)
        {
            _output = output;
        }

        public void Register(CommandLineBuilder builder)
        {
            var cmd = new Command("run", "Runs a lucy script");
            cmd.AddArgument(new Argument<FileInfo>("input-file", "The main file to execute."));
            cmd.Handler = CommandHandler.Create<FileInfo>(Run);
            builder.AddCommand(cmd);
        }

        private async Task<int> Run(FileInfo? inputFile)
        {
            if (inputFile == null)
                throw new CliException("No input file was provided.");

            if (!inputFile.Exists)
                throw new CliException($"Could not find input file \"{inputFile}\".");

            if (inputFile.DirectoryName == null)
                throw new CliException($"Could not determin directory of input file \"{inputFile}\".");

            var workspace = await Workspace.CreateFromPath(inputFile.DirectoryName);
            var workspaceProcessor = new WorkspaceProcessor(workspace);

            var mainFile = workspace.Get("/" + inputFile.Name);
            if (mainFile == null)
                throw new CliException($"Could not find main script /\"{inputFile.DirectoryName}\" in parsed workspace.");

            foreach (var doc in workspaceProcessor.Documents)
            {
                Console.WriteLine(doc.Path);
                foreach (var issue in doc.SyntaxTree?.Issues ?? new List<Issue>())
                    Console.WriteLine("    " + issue.Message);
            }

            var tempFileName = Path.ChangeExtension(Path.GetTempFileName(), "exe");
            await WinExecutableEmitter.Compile(workspaceProcessor, tempFileName);

            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = tempFileName;
            p.Start();
            await p.WaitForExitAsync();
            File.Delete(tempFileName);
            return p.ExitCode;
        }
    }
}
