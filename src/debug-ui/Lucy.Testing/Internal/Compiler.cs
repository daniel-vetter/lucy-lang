using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lucy.Testing.Internal
{
    internal static class Compiler
    {
        public static async Task CompileCurrentCompiler()
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo();
            p.StartInfo.FileName = @"dotnet";
            p.StartInfo.Arguments = "publish -o \"" + GetCompilerDir() + "\"";
            p.StartInfo.WorkingDirectory = Helper.FindFolderUpwards("compiler\\Lucy.App");
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            await p.WaitForExitAsync();

            if (p.ExitCode != 0)
                throw new Exception("Could not compile compiler");
        }

        private static string GetCompilerDir()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LucyDebugUITestCompiler");
        }

        internal static async Task<CompilerResult> Run(string mainFile, string workingDirectory)
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo();
            p.StartInfo.FileName = Path.Combine(GetCompilerDir(), @"Lucy.App.exe");
            p.StartInfo.Arguments = "run \"" + mainFile + "\"";
            p.StartInfo.WorkingDirectory = workingDirectory;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.Start();

            var stdOutTask = Read(p.StandardOutput);
            var stdErrTask = Read(p.StandardError);
            
            await p.WaitForExitAsync();

            return new CompilerResult(await stdOutTask, await stdErrTask);
        }

        private static async Task<string> Read(StreamReader stream)
        {
            var ms = new MemoryStream();
            var buffer = new byte[1024 * 4];

            while (true)
            {
                var len = await stream.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                if (len <= 0)
                    break;
                await ms.WriteAsync(buffer, 0, len);
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }

    public record CompilerResult(string StandardOutput, string ErrorOutput);
}
