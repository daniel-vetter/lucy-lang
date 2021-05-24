using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.ProjectManagement;

namespace Lucy.Feature.LanguageServer.Services
{
    [Service]
    public class SyntaxTreeDumper
    {
        internal Task Update(Workspace workspace)
        {
            /*
            var dir = Environment.GetEnvironmentVariable("LUCY_LANGUAGE_SERVER_EXPORT");
            if (string.IsNullOrWhiteSpace(dir))
                return;

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            foreach (var parsedDocument in workspaceProcessor.Documents)
            {
                var filePath = Path.ChangeExtension(Path.Combine(dir, parsedDocument.Path.Substring(1)), ".json");
                var dirPath = Path.GetDirectoryName(filePath) ?? throw new Exception("Could not determin directory path");
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(SyntaxTreeToExportJsonConverer.Convert(parsedDocument.ParserResult.RootNode), Formatting.Indented));
            }
            */

            return Task.CompletedTask;
        }
    }
}
