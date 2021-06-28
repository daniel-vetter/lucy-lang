using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Lucy.Assembler.TestCaseGenerator.Services
{
    public static class Nasm
    {
        private static int _lastId;
        private static bool _isWindows = OperatingSystem.IsWindows();
        private static Lazy<string> _nasmBinaryPath = new Lazy<string>(FindNasmBinary);

        public static async Task<NasmResponse> Run(string asmScript)
        {
            var id = Interlocked.Increment(ref _lastId);

            var tempPath = _isWindows ? "C:\\temp" : "/";
            string tempInFile = Path.Combine(tempPath, id + ".asm");
            string tempOutFile = Path.Combine(tempPath, id + ".out");
            string tempErrFile = Path.Combine(tempPath, id + ".err");
            string tempLstFile = Path.Combine(tempPath, id + ".lst");
            
            try
            {
                await File.WriteAllTextAsync(tempInFile, asmScript);

                using var p = new Process();
                p.StartInfo.FileName = _nasmBinaryPath.Value;
                p.StartInfo.Arguments = $"-f bin -O0 -Z{tempErrFile} -l {tempLstFile} -werror -o {tempOutFile} {tempInFile}";
                p.Start();
                await p.WaitForExitAsync();

                var assemblingErrors = await ParseErrorFile(tempErrFile, tempInFile);
                var listing = await ParseListingFile(tempLstFile);

                return new NasmResponse(
                    Success: p.ExitCode == 0,
                    Output: File.Exists(tempOutFile) ? await File.ReadAllBytesAsync(tempOutFile) : null,
                    AssemblingErrors: assemblingErrors.ToImmutableArray(),
                    Listing: listing
                );
            }
            finally
            {
                if (File.Exists(tempInFile)) File.Delete(tempInFile);
                if (File.Exists(tempOutFile)) File.Delete(tempOutFile);
                if (File.Exists(tempErrFile)) File.Delete(tempErrFile);
                if (File.Exists(tempLstFile)) File.Delete(tempLstFile);
            }
        }

        private static async Task<string[]> ReadAllLines(string path)
        {
            int tries = 0;
            while (true)
            {
                try
                {
                    return await File.ReadAllLinesAsync(path);
                }
                catch (Exception) when(tries < 6)
                {
                    tries++;
                    await Task.Delay(1000);
                }
            }
        }

        private static async Task<ImmutableArray<NasmIssue>> ParseErrorFile(string errorFilePath, string asmFilePath)
        {
            var b = ImmutableArray.CreateBuilder<NasmIssue>();
            var errorLines = await ReadAllLines(errorFilePath);
            for (int i = 0; i < errorLines.Length; i++)
            {
                string? line = errorLines[i];

                if (!line.StartsWith(asmFilePath))
                    throw new Exception("Unexpected file format");

                line = line.Substring(asmFilePath.Length + 1);
                var lineNumberEnd = line.IndexOf(':');
                var lineNumber = int.Parse(line.Substring(0, lineNumberEnd));
                line = line.Substring(lineNumberEnd + 1);

                var typeEnd = line.IndexOf(':');
                line = line.Substring(typeEnd + 1);

                var message = line.Trim();
                b.Add(new NasmIssue(lineNumber, message));
            }
            return b.ToImmutable();
        }

        private static async Task<NasmListing?> ParseListingFile(string listingFilePath)
        {
            if (!File.Exists(listingFilePath))
                return null;

            var lstLines = await ReadAllLines(listingFilePath);
            var issues = ImmutableArray.CreateBuilder<NasmIssue>();
            var binary = new Dictionary<int, string>();
            foreach (var line in lstLines)
            {
                var lineNumber = int.Parse(line.Substring(0, 6));
                var data = line.Substring(16, 18);
                var msg = line.Length >= 40 ? line.Substring(40) : "";


                if (string.IsNullOrWhiteSpace(data))
                {
                    continue;
                }
                else if (data == "******************")
                {
                    if (msg.StartsWith(" error: "))
                    {
                        issues.Add(new NasmIssue(lineNumber, msg.Substring(8)));
                    }
                    else if (msg.StartsWith(" warning: "))
                    {
                        issues.Add(new NasmIssue(lineNumber, msg.Substring(10)));
                    }
                    else throw new Exception("Did not expect: " + msg);
                }
                else
                {
                    data = data.Replace("[", "");
                    data = data.Replace("]", "");
                    data = data.Replace("(", "");
                    data = data.Replace(")", "");
                    data = data.Replace("-", "");
                    data = data.Replace(" ", "");

                    if (binary.TryGetValue(lineNumber, out var existing))
                    {
                        binary[lineNumber] = existing + data;
                    }
                    else
                        binary[lineNumber] = data;
                }                
            }

            return new NasmListing(binary.Select(x => new NasmBinaryEntry(x.Key, x.Value)).ToImmutableArray(), issues.ToImmutable());
        }

        private static string FindNasmBinary()
        {
            if (OperatingSystem.IsWindows())
                return PathHelper.FindUpwards("Nasm\\nasm.exe") ?? throw new Exception("Could not find nasm.exe");

            if (OperatingSystem.IsLinux())
                return "nasm";

            throw new NotSupportedException("Unsupported operating system");
        }
    }

    public record NasmResponse(bool Success, byte[]? Output, ImmutableArray<NasmIssue> AssemblingErrors, NasmListing? Listing);
    public record NasmListing(ImmutableArray<NasmBinaryEntry> Binaries, ImmutableArray<NasmIssue> Errors);
    public record NasmIssue(int LineNumber, string Message);
    public record NasmBinaryEntry(int LineNumber, string Binary);
}
