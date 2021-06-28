using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;

namespace Lucy.Assembler.Tests
{
    public static class TestCaseDataHelper
    {
        public static ImmutableArray<string> GetPaths(string filePattern)
        {
            return Directory.GetFiles(FindUpwards("Cases"), filePattern, SearchOption.AllDirectories).ToImmutableArray();
        }

        private static string FindUpwards(string path)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception("Could not find application path");
            while (true)
            {
                var testPath = Path.Combine(dir, path);
                if (File.Exists(testPath) || Directory.Exists(testPath))
                    return testPath;

                var root = Directory.GetDirectoryRoot(dir);
                if (root == dir || root == "\\")
                    throw new Exception("Could not find path: " + path);

                dir = Path.GetFullPath(Path.Combine(dir, ".."));
            }
        }
    }

    public record TestCaseFile(ImmutableList<string> Path, string Content);
}
