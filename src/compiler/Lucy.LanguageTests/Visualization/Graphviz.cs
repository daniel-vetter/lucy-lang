using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lucy.LanguageTests.Visualization
{
    public static class Graphviz
    {
        private static Task<string> GetGraphvizDirectory()
        {
            var entryAssembly = Assembly.GetEntryAssembly() ?? throw new Exception("Could not find entry assembly");
            var appDir = Path.GetDirectoryName(entryAssembly.Location) ?? throw new Exception("Could not find entry assembly directory");
            var zipFile = Directory.GetFiles(appDir, "graphviz-*").FirstOrDefault() ?? throw new Exception("Could not find graphviz zip file");

            var tempDir = Path.Combine(Path.GetTempPath(), "visualizer-" + Path.GetFileNameWithoutExtension(zipFile));
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
                ZipFile.ExtractToDirectory(zipFile, tempDir);
            }

            if (Directory.Exists(Path.Combine(tempDir, "graphviz", "bin")))
                tempDir = Path.Combine(tempDir, "graphviz", "bin");

            return Task.FromResult(tempDir);
        }


        public static async Task<byte[]> CreateSvgFromDotFile(string dotFile)
        {
            var path = await GetGraphvizDirectory();

            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo = new ProcessStartInfo();
            p.StartInfo.FileName = Path.Combine(path, "dot.exe");
            p.StartInfo.Arguments = "-Tsvg";
            p.StartInfo.WorkingDirectory = path;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            await p.StandardInput.WriteAsync(dotFile);
            p.StandardInput.Close();
            var ms = new MemoryStream();
            var buffer = new byte[1024 * 4];
            while (true)
            {
                var len = await p.StandardOutput.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                if (len <= 0)
                    break;
                await ms.WriteAsync(buffer, 0, len);
            }
            await p.WaitForExitAsync();
            return ms.ToArray();
        }
    }

    public class TableNode
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public List<Property> Properties = new List<Property>();
        public string Color { get; set; } = "FFFFFF";

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Id + " [ ");
            sb.Append($"label = <{ CreateTable()}> style = \"filled\" color = \"#{Color}\" fillcolor = \"#{Color}10\" ");
            sb.Append("]");
            return sb.ToString();
        }

        private string CreateTable()
        {
            var sb = new StringBuilder();
            sb.Append("<TABLE BORDER=\"0\">");
            sb.Append($"<TR><TD COLSPAN=\"2\"><FONT POINT-SIZE=\"14\"><B>{Mask(Title)}</B></FONT></TD></TR>");
            foreach (var p in Properties)
            {
                sb.Append($"<TR><TD CELLSPACING=\"0\" CELLPADDING=\"0\" ALIGN=\"LEFT\"><B><FONT COLOR=\"#{p.Color}\">{Mask(p.Label)}</FONT></B></TD><TD ALIGN=\"LEFT\"><FONT COLOR=\"#{p.Color}\">{Mask(p.Value)}</FONT></TD></TR>");
            }

            sb.Append("</TABLE>");
            return sb.ToString();
        }

        private string Mask(string value)
        {
            value = value.Replace("<", "&lt;").Replace(">", "&gt;");
            if (value.Replace("\r", "").Replace("\n", "").Length == 0)
                value = " " + value;
            return value;
        }
    }

    public record Property(string Label, string Value, string Color = "000000");

}
