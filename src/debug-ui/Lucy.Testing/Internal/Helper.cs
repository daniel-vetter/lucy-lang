using System;
using System.IO;

namespace Lucy.Testing.Internal
{
    internal static class Helper
    {
        public static string FindFolderUpwards(string searchFor)
        {
            var dir = AppContext.BaseDirectory;
            while (true)
            {
                var testPath = Path.Combine(dir, searchFor);
                if (Directory.Exists(testPath))
                    return testPath;

                dir = Path.GetDirectoryName(dir) ?? throw new Exception("Could not extract directory from " + dir); ;
            }
        }
    }
}
