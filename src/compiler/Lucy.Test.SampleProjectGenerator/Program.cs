using System.Text;

namespace Lucy.Test.SampleProjectGenerator
{
    internal class Program
    {
        private readonly Random _r = new(0);
        private readonly NameProvider _np = new();

        static void Main(string[] args)
        {
            new Program().Run();
        }

        private void Run()
        {
            var np = new NameProvider();

            var p = new Project();

            for (int i = 0; i < 10; i++)
                p.Files.Add(CreateFile(p, "", 0));


            Write(p);

        }

        private void Write(Project project)
        {
            var baseDir = "C:\\lucy-sample-project";
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);
            else
            {
                foreach(var file in Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories))
                    File.Delete(file);
                foreach(var dir in Directory.GetDirectories(baseDir, "*"))
                    Directory.Delete(dir, true);

            }
            
            foreach (var projectFile in project.Files)
            {
                var fullPath = Path.GetFullPath(Path.Combine(baseDir, projectFile.DocumentPath[1..]));
                var dir = Path.GetDirectoryName(fullPath);

                Directory.CreateDirectory(dir);
                var sb = new StringBuilder();

                foreach (var import in projectFile.Imports)
                    sb.AppendLine("import \""+import[..^5]+"\"");

                sb.AppendLine();

                foreach (var projectFileFunction in projectFile.Functions)
                {
                    sb.AppendLine("fun " + projectFileFunction.Name + "(): void {");
                    foreach (var call in projectFileFunction.Calls)
                    {
                        sb.AppendLine("    " + call + "()");
                    }
                    sb.AppendLine("}");
                    sb.AppendLine("");

                }
                File.WriteAllText(fullPath, sb.ToString());
            }
        }

        private ProjectFile CreateFile(Project project, string path, int depth)
        {
            if (_r.Next(5) == 0)
            {
                path += "/" + _np.Get("folder");
            }
            
            var subFiles = new List<ProjectFile>();
            for (int i = 0; i < _r.Next(0, 8 - depth); i++)
                subFiles.Add(CreateFile(project, path, depth + 1));
            foreach (var subFile in subFiles)
            {
                project.Files.Add(subFile);
            }

            var file = new ProjectFile
            {
                DocumentPath = path + "/" + _np.Get("file") + ".lucy",
                Imports = subFiles.Select(x => x.DocumentPath).ToList()
            };

            var subFunctions = subFiles.SelectMany(x => x.Functions)
                .Select(x => x.Name)
                .ToArray();

            for (int i = 0; i < _r.Next(1, 30); i++)
            {
                file.Functions.Add(new Function
                {
                    Name = _np.Get("function"),
                    Calls = subFunctions.Length == 0 ? new List<string>() : Enumerable.Range(0, _r.Next(0, 50)).Select(_ => subFunctions[_r.Next(0, subFunctions.Length)]).ToList()
                });
            }
            
            return file;
        }
    }

    public class NameProvider
    {
        private readonly Dictionary<string, int> _counter = new();

        public string Get(string category)
        {
            if (!_counter.ContainsKey(category))
                _counter[category] = 0;
            _counter[category]++;

            return category + _counter[category];
        }
    }


    public class Project
    {
        public List<ProjectFile> Files { get; set; } = new();
    }

    public class ProjectFile
    {
        public required string DocumentPath { get; set; }
        public List<string> Imports { get; set; } = new();
        public List<Function> Functions { get; set; } = new();
    }

    public class Function
    {
        public required string Name { get; set; }
        public List<string> Calls { get; set; } = new();
    }
}