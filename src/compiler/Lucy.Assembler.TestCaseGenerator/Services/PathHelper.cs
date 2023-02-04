using System;
using System.IO;
using System.Reflection;

namespace Lucy.Assembler.TestCaseGenerator.Services
{
    public static class PathHelper
    {
        public static string FindUpwards(string path)
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
}
