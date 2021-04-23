using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

namespace Lucy.Testing.Internal
{
    internal static class TestCaseDiscovery
    {
        public static async Task<ImmutableArray<TestCase>> FindTestCases()
        {
            var folder = Helper.FindFolderUpwards("Lucy.Testing\\Cases");
            var files = Directory.GetFiles(folder, "testCase.json", SearchOption.AllDirectories);
            var result = ImmutableArray.CreateBuilder<TestCase>();
            foreach (var testCasePath in files)
            {
                var testCaseDir = Path.GetDirectoryName(testCasePath) ?? throw new Exception("Could not extract directory path from: " + testCasePath);
                var json = JObject.Parse(await File.ReadAllTextAsync(testCasePath));

                var name = Path.GetDirectoryName(testCasePath.Substring(folder.Length + 1)) ?? throw new Exception("Could not parse test name");
                name = name.Replace("\\", "/");

                var expectedOutput = json["expectedOutput"]?.Value<string>();
                var expectedErrorMessage = json["expectedErrors"]?.Values<string>().ToImmutableArray() ?? ImmutableArray<string>.Empty;
                var parseOnly = json["parseOnly"]?.Value<bool>() ?? false;
                result.Add(new TestCase(name, testCaseDir, expectedOutput, expectedErrorMessage, parseOnly));
            }

            return result.ToImmutable();
        }
    }
}
