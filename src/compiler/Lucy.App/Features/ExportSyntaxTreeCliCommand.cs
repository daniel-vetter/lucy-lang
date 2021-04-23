using Lucy.App.Infrastructure.Cli;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.ProjectManagement;
using Lucy.Feature.LanguageServer.Services;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lucy.App.Features
{
    [Service]
    class ExportSyntaxTreeCliCommand : ICliCommand
    {
        public void Register(CommandLineBuilder builder)
        {
            var cmd = new Command("export-syntax-tree", "Parses a lucy workspace and exports all syntax trees as json files.");
            cmd.AddArgument(new Argument<FileInfo>("input-file", "The main file to parse."));
            cmd.AddArgument(new Argument<string>("outputDirectory", "The output directory where all json files will be placed."));
            cmd.Handler = CommandHandler.Create<FileInfo, string>(ExportSyntaxTree);
            builder.AddCommand(cmd);
        }

        private async Task ExportSyntaxTree(FileInfo? inputFile, string? outputDirectory)
        {
            if (inputFile == null)
                throw new CliException("No input file was provided.");

            if (outputDirectory == null)
                throw new CliException("No output directory was provided.");

            if (!inputFile.Exists)
                throw new CliException($"Could not find input file \"{inputFile}\".");

            if (inputFile.DirectoryName == null)
                throw new CliException($"Could not determin directory of input file \"{inputFile}\".");

            var workspace = await Workspace.CreateFromPath(inputFile.DirectoryName);
            var workspaceProcessor = new WorkspaceProcessor(workspace);

            var fullOutputPath = Path.GetFullPath(outputDirectory);

            if (!Directory.Exists(fullOutputPath))
                Directory.CreateDirectory(fullOutputPath);

            foreach(var document in workspace.Documents)
            {
                /*
                var parsedResult = workspaceProcessor.Documents.Single(x => x.Path == document.Path);
                var targetPath = Path.ChangeExtension(Path.Combine(fullOutputPath, document.Path.Substring(1)), ".json");
                var json = SyntaxTreeToExportJsonConverer.Convert(parsedResult.ParserResult.RootNode);
                await File.WriteAllTextAsync(targetPath, json.ToString());
                */
            }
        }
    }
}
