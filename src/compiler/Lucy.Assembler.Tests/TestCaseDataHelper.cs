using NUnit.Framework;
using System;
using System.Collections.Immutable;
using System.IO;

namespace Lucy.Assembler.Tests
{
    public static class TestCaseDataHelper
    {
        public static ImmutableArray<string> GetPaths(string filePattern)
        {
            var dir = Path.GetFullPath(TestContext.CurrentContext.TestDirectory);
            while (!Directory.Exists(Path.Combine(dir, "Cases")))
            {
                Console.WriteLine("Checking " + dir);
                dir = Path.GetFullPath(Path.Combine(dir, ".."));
            }

            dir = Path.GetFullPath(Path.Combine(dir, "Cases"));
            var files = Directory.GetFiles(dir, filePattern, SearchOption.AllDirectories);

            var result = ImmutableArray.CreateBuilder<string>();
            foreach (var file in files)
                result.Add(file);
            return result.ToImmutable();
        }
    }

    public record TestCaseFile(ImmutableList<string> Path, string Content);
}
