using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Lucy.DebugUI.Services
{
    public class Watcher
    {
        public static IObservable<Message> Start(string path)
        {
            return Observable.Create<Message>(async (obs, ct) =>
            {
                await SaveLastUsedPath(path);

                var fileWasReportedMissing = false;
                var lineReader = new LineReader(path);
                var fileSize = 0;

                obs.OnNext(new ResetMessage());

                while (!ct.IsCancellationRequested)
                {
                    var fileExists = File.Exists(path);
                    if (fileWasReportedMissing && fileExists)
                    {
                        obs.OnNext(new ResetMessage());
                        fileWasReportedMissing = false;
                    }

                    if (!fileWasReportedMissing && !fileExists)
                    {
                        fileWasReportedMissing = true;
                        fileSize = 0;
                        lineReader = new LineReader(path);
                        obs.OnNext(new FileDoesNotExistMessage());
                    }


                    if (!fileExists)
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    try
                    {
                        var newFileSize = (int)new FileInfo(path).Length;
                        if (newFileSize < fileSize)
                        {
                            obs.OnNext(new ResetMessage());
                            lineReader = new LineReader(path);
                        }
                        fileSize = newFileSize;

                        var lines = await lineReader.ReadMoreFromFile();
                        foreach (var line in lines)
                            obs.OnNext(new TraceEventMessage(line));
                    }
                    catch (FileNotFoundException)
                    {
                        if (!fileWasReportedMissing)
                        {
                            fileWasReportedMissing = true;
                            fileSize = 0;
                            lineReader = new LineReader(path);
                            obs.OnNext(new FileDoesNotExistMessage());
                        }
                    }

                    await Task.Delay(50);
                }
            });
        }

        public static async Task SaveLastUsedPath(string path)
        {
            var dir = Path.Combine("Lucy.DebugUI", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            await File.WriteAllTextAsync(Path.Combine(dir, "last-path.txt"), path);
        }

        public static async Task<string> ReadLastUsedPath()
        {
            var file = Path.Combine("Lucy.DebugUI", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "last-path.txt");
            if (File.Exists(file))
                return await File.ReadAllTextAsync(file);
            return "";
        }
    }

    public class TraceEventMessage : Message
    {
        public TraceEventMessage(string value)
        {
            Value = value;
        }

        public string Value { get; private set; }
    }

    public class Message
    {

    }

    public class FileDoesNotExistMessage : Message
    {

    }

    public class ResetMessage : Message
    {

    }
}
