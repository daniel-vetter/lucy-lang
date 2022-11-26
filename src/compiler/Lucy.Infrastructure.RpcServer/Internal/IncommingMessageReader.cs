using System;
using System.Buffers;
using System.Buffers.Text;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;
using Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

namespace Lucy.Infrastructure.RpcServer.Internal;

[Service(Lifetime.Singleton)]
public class IncommingMessageReader
{
    private readonly JsonRpcConfig _config;
    private readonly MessageConverter _messageConverter;
    private readonly JsonRpcSerializer _serializer;
    private readonly PipeReader _reader;

    public IncommingMessageReader(JsonRpcConfig config, MessageConverter messageConverter, JsonRpcSerializer serializer)
    {
        _config = config;
        _messageConverter = messageConverter;
        _serializer = serializer;
        _reader = PipeReader.Create(Console.OpenStandardInput());
    }

    static ReadOnlySpan<byte> NewLine => new[] { (byte)'\r', (byte)'\n' };
    static ReadOnlySpan<byte> Colon => new[] { (byte)':' };
    static ReadOnlySpan<byte> HeaderContentLength => Encoding.UTF8.GetBytes("Content-Length");

    public async Task<Message?> ReadNext(CancellationToken cancellationToken)
    {
        int contentLength = -1;
        bool headersAreDone = false;

        while (true)
        {
            ReadResult? result;
            try
            {
                result = await _reader.ReadAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            if (!result.HasValue)
                throw new Exception("No ReadResult received.");

            var buffer = result.Value.Buffer;

            while (!headersAreDone && TryReadLine(ref buffer, out ReadOnlySequence<byte> line))
            {
                if (line.Length == 0)
                {
                    headersAreDone = true;
                    continue;
                }

                if (!TrySplitHeader(ref line, out var name, out var value))
                    throw new Exception("Expected valid header");

                if (HasSameContent(name, HeaderContentLength))
                    if (!TryParseNumber(value, out contentLength))
                        throw new Exception("Could not parse header: " + Encoding.UTF8.GetString(HeaderContentLength));
            }

            if (headersAreDone && buffer.Length >= contentLength)
            {
                var message = ReadJson(ref buffer, contentLength);
                _reader.AdvanceTo(buffer.Start, buffer.End);
                if (_config.TraceTarget != null)
                    await _config.TraceTarget.OnIncomingMessage(message, _serializer);
                return message;
            }

            _reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.Value.IsCompleted)
                return null;
        }
    }

    private Message ReadJson(ref ReadOnlySequence<byte> buffer, int length)
    {
        var bytes = buffer.Slice(0, length).ToArray();
        var msg = _messageConverter.FromBytes(bytes);
        buffer = buffer.Slice(length);
        return msg;
    }

    private static bool HasSameContent(ReadOnlySequence<byte> left, ReadOnlySpan<byte> right)
    {
        if (left.IsSingleSegment)
            return right.SequenceEqual(left.FirstSpan);

        int offset = 0;
        foreach (var memory in left)
        {
            if (!memory.Span.SequenceEqual(right.Slice(offset, memory.Span.Length)))
                return false;
            offset += memory.Span.Length;
        }
        return true;
    }

    private static bool TryParseNumber(ReadOnlySequence<byte> value, out int result)
    {
        if (value.IsSingleSegment)
        {
            if (Utf8Parser.TryParse(value.FirstSpan, out result, out _))
            {
                return true;
            }
        }
        else
        {
            if (value.Length < 32)
            {
                Span<byte> valueSpan = stackalloc byte[(int)value.Length];
                value.CopyTo(valueSpan);
                if (Utf8Parser.TryParse(value.FirstSpan, out result, out _))
                {
                    return true;
                }
            }
        }

        result = default;
        return false;
    }

    private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        var reader = new SequenceReader<byte>(buffer);

        if (reader.TryReadTo(out line, NewLine))
        {
            buffer = buffer.Slice(reader.Position);
            return true;
        }

        line = default;
        return false;
    }

    private static bool TrySplitHeader(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> name, out ReadOnlySequence<byte> value)
    {
        var reader = new SequenceReader<byte>(buffer);
        if (reader.TryReadTo(out name, Colon))
        {
            reader.AdvancePast((byte)' ');
            value = buffer.Slice(reader.Position);
            buffer = buffer.Slice(buffer.End);
            return true;
        }
        name = default;
        value = default;
        return false;
    }
}