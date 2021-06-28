using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Lucy.Assembler.TestCaseGenerator.Services
{
    public static class TestCaseProcessor
    {
        public static async Task Run(ChannelReader<StatementTest> reader, ChannelWriter<StatementTest> writer, int bits)
        {
            await Task.WhenAll(Enumerable.Range(0, 50).Select(_ => Process(reader, writer, bits)).ToArray());
            writer.Complete();
        }

        private static async Task Process(ChannelReader<StatementTest> reader, ChannelWriter<StatementTest> writer, int bits)
        {
            var currentBatch = new List<StatementTest>();
            while (await FillBatch(reader, currentBatch))
            {
                await ProcessBatch(currentBatch, bits, writer);
            }
        }

        private static async Task ProcessBatch(List<StatementTest> currentBatch, int bits, ChannelWriter<StatementTest> writer)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BITS " + bits);
            sb.AppendLine("label:");
            sb.AppendLine("");
            foreach (var test in currentBatch)
                sb.AppendLine(test.Text);

            var result = await Nasm.Run(sb.ToString());

            foreach(var error in result.AssemblingErrors)
                currentBatch[error.LineNumber - 4].Errors.Add(error.Message);

            if (result.Listing != null)
            {
                foreach (var error in result.Listing.Errors)
                    currentBatch[error.LineNumber - 4].Errors.Add(error.Message);

                foreach (var lstEntry in result.Listing.Binaries)
                    currentBatch[lstEntry.LineNumber - 4].Binary = lstEntry.Binary;
            }

            foreach(var item in currentBatch)
                if (item.Errors.Count > 0 || item.Binary != null)
                    await writer.WriteAsync(item);
            var count = currentBatch.RemoveAll(x => x.Errors.Count > 0 || x.Binary != null);
        }

        private static async Task<bool> FillBatch(ChannelReader<StatementTest> reader, List<StatementTest> currentBatch)
        {
            while (await reader.WaitToReadAsync())
            {
                while (reader.TryRead(out var item))
                {
                    currentBatch.Add(item);
                    if (currentBatch.Count >= 10000)
                        return true;
                }
            }

            return currentBatch.Count > 0;
        }
    }
}
