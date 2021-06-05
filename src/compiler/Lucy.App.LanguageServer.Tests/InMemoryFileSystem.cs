using Lucy.App.LanguageServer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Tests
{
    internal class InMemoryFileSystem : IFileSystem
    {
        Directory _root = new Directory("", null);

        public Task<SystemPath[]> GetFilesInDirectory(SystemPath path)
        {
            var files = GetDirectory(path, false)
                .Children
                .OfType<File>()
                .Select(x => GetSystemPath(x))
                .ToArray();

            return Task.FromResult(files);
        }

        private SystemPath GetSystemPath(Entry entry)
        {
            var list = new List<Entry>();
            var cur = entry;
            while (cur != null)
            {
                list.Add(cur);
                cur = cur.Parent;
            }

            list.Reverse();
            return new SystemPath(string.Join("\\", list.Select(x => x.Name)));
        }

        private Directory GetDirectory(SystemPath path, bool createIfNotExist)
        {
            var dir = _root;
            foreach(var name in path.Elements)
            {
                var subEntry = dir.Children.SingleOrDefault(x => x.Name == name);

                if (subEntry is File)
                    throw new Exception("Could not find path: " + path);

                if (subEntry == null)
                {
                    if (!createIfNotExist)
                        throw new Exception("Could not find path: " + path);
                    var newDir = new Directory(name, dir);
                    dir.Children.Add(newDir);
                    dir = newDir;
                    continue;
                }

                if (subEntry is Directory subDir)
                {
                    dir = subDir;
                    continue;
                }
                    
                throw new NotSupportedException();
            }
            return dir;
        }

        private File GetFile(SystemPath path, bool createIfNotExist)
        {
            var dir = GetDirectory(path.SubPath(0, -1), false);
            var file = dir.Children.SingleOrDefault(x => x.Name == path.Elements[^1]);

            if (file is Directory)
                throw new Exception("Could not find path: " + path);
            
            if (file == null)
            {
                if (!createIfNotExist)
                    throw new Exception("Could not find path: " + path);
                var newFile = new File(path.Elements[^1], dir);
                dir.Children.Add(newFile);
                return newFile;
            }

            throw new NotSupportedException();
        }

        public Task<byte[]> ReadAllBytes(SystemPath path)
        {
            return Task.FromResult(GetFile(path, false).Data.ToArray());
        }

        public Task<string> ReadAllText(SystemPath file)
        {
            return Task.FromResult(Encoding.UTF8.GetString(GetFile(file, false).Data));
        }

        public Task WriteAllBytes(SystemPath path, byte[] data)
        {
            GetFile(path, true).Data = data.ToArray();
            return Task.CompletedTask;
        }

        public Task WriteAllText(SystemPath file, string content)
        {
            GetFile(file, true).Data = Encoding.UTF8.GetBytes(content);
            return Task.CompletedTask;
        }

        public void CreateDirectory(SystemPath path)
        {
            GetDirectory(path, true);
        }

        private abstract class Entry
        {
            public Entry(string name, Directory? parent)
            {
                Name = name;
                Parent = parent;
            }

            public string Name { get; } = "";
            public Directory? Parent { get; }
        }

        private class Directory : Entry
        {
            public Directory(string name, Directory? parent) : base(name, parent)
            {
            }

            public List<Entry> Children { get; } = new List<Entry>();
        }

        private class File : Entry
        {
            public File(string name, Directory parent) : base(name, parent)
            {
            }

            public byte[] Data { get; set; } = Array.Empty<byte>();
        }
    }

}
