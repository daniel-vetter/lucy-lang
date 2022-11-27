using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Lucy.App.LanguageServer.Infrastructure
{
    internal static class DebugVsCodeRunner
    {
        public static bool IsVsCodeStartupRequested => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LUCY_LANGUAGE_SERVER_RUN_VSCODE_EXTENSION"));

        public static IPEndPoint NetworkEndpoint => IPEndPoint.Parse("127.0.0.1:5997");

        public static void Launch()
        {
            var appDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var vsCodeDir = Path.Combine(appDir, "Programs\\Microsoft VS Code\\code.exe");


            var info = new ProcessStartInfo(vsCodeDir)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                EnvironmentVariables =
                {
                    ["LUCY_LANGUAGE_SERVER_NETWORK_ENDPOINT_PORT"] = NetworkEndpoint.Port.ToString()
                },
                ArgumentList =
                {
                    { Environment.GetEnvironmentVariable("LUCY_LANGUAGE_SERVER_RUN_VSCODE_EXTENSION_WORKSPACE") },
                    { "--extensionDevelopmentPath=" + Environment.GetEnvironmentVariable("LUCY_LANGUAGE_SERVER_RUN_VSCODE_EXTENSION") }
                }

            };

            var p = Process.Start(info);
            if (p == null)
                throw new Exception("Could not start vscode");
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
        }
    }
}
