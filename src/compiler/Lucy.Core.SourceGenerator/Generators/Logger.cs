﻿namespace Lucy.Core.SourceGenerator.Generators;

internal class Logger
{
    private readonly string? _name;
    private readonly bool _enabled;

    public Logger(string name, bool enabled)
    {
        if (enabled)
        {
            if (!Directory.Exists("C:\\generator-log"))
                Directory.CreateDirectory("C:\\generator-log");

            _name = DateTimeOffset.Now.ToString("yyyy-MM-dd-HH-mm-ss") + " " + name + ".log";
        }
        else
            _name = null;

        _enabled = enabled;
    }

    public bool IsEnabled => _enabled;

    public void Write(string message)
    {
        if (!_enabled || _name == null)
            return;

        using var file = File.OpenWrite(Path.Combine("C:\\generator-log", _name));
        using var sw = new StreamWriter(file);

        file.Seek(0, SeekOrigin.End);
        sw.WriteLine(DateTimeOffset.UtcNow + " " + message);
        sw.Flush();
        file.Flush();
    }
}