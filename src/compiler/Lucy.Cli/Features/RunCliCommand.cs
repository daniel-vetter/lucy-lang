using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading.Tasks;
using Lucy.App.Cli.Infrastructure.Cli;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;
using Lucy.Interpreter;

namespace Lucy.App.Cli.Features
{
    [Service(Lifetime.Singleton)]
    class RunCliCommand : ICliCommand
    {
        public void Register(CommandLineBuilder builder)
        {
            var cmd = new Command("run", "Runs a lucy script");
            cmd.AddOption(new("--use-interpreter", "The application will be executed by the interpreter instead of being build into a binary."));
            cmd.Handler = CommandHandler.Create((bool useInterpreter) => Run(useInterpreter));
            builder.AddCommand(cmd);
        }

        private async Task<int> Run(bool useInterpreter)
        {
            var workspace = await Workspace.CreateFromPath(Environment.CurrentDirectory);

            return useInterpreter 
                ? Interpret(workspace) 
                : await CompileAndRun();
        }

        private int Interpret(Workspace workspace)
        {
            if (workspace.Documents.Count() > 1)
                throw new CliException("Interpreter currently only supports single file projects.");

            if (workspace.Documents.Count() == 0)
                throw new Exception("Could not find a code file in the workspace.");

            using var semanticAnalyzer = new SemanticAnalyzer(workspace);

            if (CheckForErrors(workspace, semanticAnalyzer))
                return -1;

            var result = CodeInterpreter.Run(semanticAnalyzer);

            return result switch
            {
                VoidValue => 0,
                NumberValue nv => (int)nv.Value,
                _ => throw new Exception("Application did not return a valid exit code.")
            };
        }

        private static bool CheckForErrors(Workspace workspace, SemanticAnalyzer semanticAnalyzer)
        {
            var errors = semanticAnalyzer.Get<AllErrorsCollector>().GetAllErrors();

            foreach (var error in errors)
            {
                var file = workspace.GetFile(error.DocumentPath);
                var range = file.LineBreakMap.To2D(error.Range);

                Console.WriteLine($"{error.DocumentPath} {range.Start}: {error.Message}");
            }

            return errors.Count > 0;
        }

        private static Task<int> CompileAndRun()
        {
            throw new NotImplementedException();
        }
    }
}
