﻿using Lucy.Common.ServiceDiscovery;
using Lucy.Core.ProjectManagement;
using Lucy.Feature.LanguageServer.Models;
using System;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Infrastructure
{
    [Service(Lifetime.Singleton)]
    public class CurrentWorkspace
    {
        private readonly IFileSystem _fileSystem;

        public Workspace? Workspace { get; private set; }
        public SystemPath? RootPath { get; private set; }

        public CurrentWorkspace(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public async Task Load(SystemPath path)
        {
            var ws = new Workspace();
            var rootPathLength = path.ToString().Length;
            var files = await _fileSystem.GetFilesInDirectory(path);
            foreach(var file in files)
            {
                ws.AddDocument(file.ToString().Substring(rootPathLength).Replace("\\", "/"), await _fileSystem.ReadAllText(file));
            }

            Workspace = ws;
            RootPath = path;

            Process();
        }

        public string ToWorkspacePath(SystemPath path)
        {
            if (RootPath == null)
                throw new Exception("No workspace path availible.");

            return path.ToString().Substring(RootPath.ToString().Length).Replace("\\", "/");
        }

        public void Process()
        {
            if (Workspace == null)
                throw new Exception("No workspace is loaded.");

            Workspace.Process();
        }
    }
}
