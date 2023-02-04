using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace Lucy.App.LanguageServer.Infrastructure
{
    internal static class DebugVsCodeRunner
    {
        public static bool IsVsCodeStartupRequested => VsCodeExtensionDir != null;

        public static IPEndPoint NetworkEndpoint => IPEndPoint.Parse("127.0.0.1:5997");

        private static string? VsCodeExtensionDir
        {
            get
            {
                var dir = Environment.GetEnvironmentVariable("LUCY_LANGUAGE_SERVER_RUN_VSCODE_EXTENSION");
                return string.IsNullOrWhiteSpace(dir) ? null : dir;
            }
        }

        public static void Launch()
        {
            var vsCodePossiblePath = new[]
            {
                "C:\\Program Files\\Microsoft VS Code\\Code.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs\\Microsoft VS Code\\code.exe")
            };

            var vsCodePath = vsCodePossiblePath
                .FirstOrDefault(File.Exists);

            if (vsCodePath == null)
                throw new Exception("Could not find vscode executable. Please make sure Visuals Studio Code is installed.");
            
            var info = new ProcessStartInfo(vsCodePath)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                EnvironmentVariables =
                {
                    ["LUCY_LANGUAGE_SERVER_NETWORK_ENDPOINT_PORT"] = NetworkEndpoint.Port.ToString()
                },
                ArgumentList =
                {
                    { "--extensionDevelopmentPath=" + VsCodeExtensionDir }
                }
            };

            var workspaceDir = Environment.GetEnvironmentVariable("LUCY_LANGUAGE_SERVER_RUN_VSCODE_EXTENSION_WORKSPACE");
            if (!string.IsNullOrWhiteSpace(workspaceDir))
                info.ArgumentList.Add(workspaceDir);

            var p = Process.Start(info);
            if (p == null)
                throw new Exception("Could not start vscode");
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
        }
    }
}
