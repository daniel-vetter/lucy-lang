using Lucy.App.Infrastructure.Cli;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Compiler;
using Lucy.Core.Interpreter;
using Lucy.Core.ProjectManagement;
using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lucy.App.Features
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
            //workspace.Process();

            return useInterpreter 
                ? Interpret(workspace) 
                : await CompileAndRun(workspace);
        }

        private int Interpret(Workspace workspace)
        {
            if (workspace.Documents.Count() > 1)
                throw new CliException("Interpreter currently only supports single file projects.");

            if (workspace.Documents.Count() == 0)
                throw new Exception("Could not find a code file in the workspace.");

            var doc = workspace.Documents.Single();

            //if (doc.SyntaxTree == null)
            //throw new Exception("Syntax tree was not parsed.");

            //if (doc.SemanticModel == null)
            //throw new Exception("Semenatic model was not provided.");

            //var ctx = new InterpreterContext(doc.SemanticModel);
            //var result = TreeInterpreter.Run(doc.SyntaxTree, ctx);
            /*
            if (result is VoidValue)
                return 0;
            else if (result is NumberValue nv)
                return (int)nv.Value;
            else throw new Exception("Application did not return a valid exit code.");
            */
            return 0;
        }

        private static async Task<int> CompileAndRun(Workspace workspace)
        {
            var tempFileName = "C:\\temp\\out.exe";
            await WinExecutableEmitter.Emit(workspace, tempFileName);
            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = tempFileName;
            p.Start();
            await p.WaitForExitAsync();
            return p.ExitCode;
        }
    }
}
