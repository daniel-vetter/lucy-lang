using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace Lucy.Common;

public static class Profiler
{
    public static void Attach()
    {
        var cmd = $"""
            dotnet trace collect -p {Process.GetCurrentProcess().Id} --output trace.netstat
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
            WorkingDirectory= dir,
            UseShellExecute = true
        });

        Thread.Sleep(5000);
    }

    private static readonly List<RecordedEvent> _events = new();
    private static readonly  Stopwatch _sw = Stopwatch.StartNew();

    [Conditional("CUSTOM_TRACE")]
    public static void Start(string name)
    {
        _events.Add(new RecordedEvent
        {
            IsStart = true,
            Name = name,
            Timestamp = _sw.Elapsed.TotalMilliseconds
        });
    }

    [Conditional("CUSTOM_TRACE")]
    public static void End(string name)
    {
        _events.Add(new RecordedEvent
        {
            IsStart = false,
            Name = name,
            Timestamp = _sw.Elapsed.TotalMilliseconds
        });
    }

    public static void ExportAndShow()
    {
        var file = new SpeedscopeFile
        {
            Name = "trace.speedscope",
            Exporter = "lucy trace profiler",
            Shared = new SpeedscopeShared()
        };

        var profile = new SpeedscopeProfile
        {
            Type = "evented",
            Name = "default",
            Unit = "milliseconds",
            StartValue = 0,
            EndValue = _sw.Elapsed.TotalMilliseconds
        };

        file.Profiles.Add(profile);

        var frameIndex = new Dictionary<string, int>();

        foreach (var e in _events)
        {
            if (!frameIndex.TryGetValue(e.Name, out var index))
            {
                index = frameIndex.Count;
                frameIndex[e.Name] = index;
                file.Shared.Frames.Add(new SpeedscopeFrame
                {
                    Name = e.Name
                });
            }

            profile.Events.Add(new SpeedscopeEvent
            {
                At = e.Timestamp,
                Type = e.IsStart ? "O" : "C",
                Frame = index
            });
        }

        var json = JsonSerializer.Serialize(file, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".speedscope.json");
        File.WriteAllText(filePath, json);
        
        Process.Start(new ProcessStartInfo("speedscope")
        {
            Arguments = filePath,
            UseShellExecute = true
        });
    }

    private class RecordedEvent
    {
        public required string Name { get; set; }
        public required bool IsStart { get; set; }
        public required double Timestamp { get; set; }
    }
}

public class SpeedscopeFile
{
    public required string Exporter { get; set; }
    public required string Name { get; set; }
    public int ActiveProfilerIndex { get; set; }
    public required SpeedscopeShared Shared { get; set; }
    public List<SpeedscopeProfile> Profiles { get; } = new();
}

public class SpeedscopeProfile
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public required string Unit { get; set; }
    public required double StartValue { get; set; }
    public required double EndValue { get; set; }
    public List<SpeedscopeEvent> Events { get; } = new();
}

public class SpeedscopeEvent
{
    public required string Type { get; set; }
    public required int Frame { get; set; }
    public required double At { get; set; }
}

public class SpeedscopeShared
{
    public List<SpeedscopeFrame> Frames { get; set; } = new();
}

public class SpeedscopeFrame
{
    public required string Name { get; set; }
}