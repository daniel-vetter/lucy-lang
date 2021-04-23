using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lucy.Core.Parser;
using Lucy.Core.Interpreter;
using Lucy.LanguageTests.Visualization;

namespace Lucy.LanguageTests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var dir = FindCasesDirectory();
            var success = 0;
            var failed = 0;

            foreach (var file in Directory.GetFiles(dir, "*.case", SearchOption.AllDirectories))
            {
                var name = file.Substring(dir.Length + 1);
                string? error = null;

                if (file.Contains("_"))
                    continue;

                try
                {
                    await RunTestCase(file);
                }
                catch (Exception e)
                {
                    error = e.Message;
                }

                if (error != null)
                    failed++;
                else
                    success++;

                WriteInfoLine(name, error);
            }

            Console.WriteLine();
            Console.WriteLine($"Total: {success + failed}, Success: {success}, Failed: {failed}");
        }

        private static async Task RunTestCase(string file)
        {
            var code = await File.ReadAllTextAsync(file);
            var result = CodeParser.Parse(code);

            if (result.Issues.Any())
            {
                throw new Exception(string.Join("\r\n", result.Issues.Select(x => x.Message)));
            }
            await Visualizer.WriteSyntaxTreeToSvg(result.RootNode, file + ".svg");

            TreeInterpreter.Run(result.RootNode, new InterpreterContext());
        }

        private static void WriteInfoLine(string name, string? error)
        {
            Console.Write("[");
            if (error == null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  ok  ");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" fail ");
            }
            Console.ResetColor();
            Console.WriteLine("] " + name);

            if (error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("         " + error);
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        private static string FindCasesDirectory()
        {
            var currentPath = Environment.CurrentDirectory;
            while (true)
            {
                if (Directory.Exists(Path.Combine(currentPath, "Cases")))
                    return Path.GetFullPath(Path.Combine(currentPath, "Cases"));

                currentPath = Path.Combine(currentPath, "..");
            }
        }
    }

}
