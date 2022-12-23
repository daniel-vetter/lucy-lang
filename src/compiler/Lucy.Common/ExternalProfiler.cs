using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Lucy.Common;

public static class ExternalProfiler
{
    public static void Attach()
    {
        var cmd = $"""
            dotnet trace collect -p {Environment.ProcessId} --output trace.netstat
            dotnet trace convert --output trace.json --format Speedscope trace.netstat
            speedscope trace.speedscope.json
            """;

        var dir = Path.Combine(Path.GetTempPath(), "lucy-trace" + Environment.TickCount);
        var file = Path.Combine(dir, "run.cmd");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(file, cmd);

        Process.Start(new ProcessStartInfo(file)
        {
            WorkingDirectory = dir,
            UseShellExecute = true
        });

        Thread.Sleep(5000);
    }
}