using System;
using Lucy.Common.ServiceDiscovery;
using Newtonsoft.Json.Linq;

namespace Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

public abstract record Message;
public abstract record ResponseMessage(long Id) : Message;

public record RequestMessage(long Id, string Method, JToken? Params) : Message;
public record ResponseSuccessMessage(long Id, JToken? Result) : ResponseMessage(Id);
public record ResponseErrorMessage(long Id, ErrorDescription Error) : ResponseMessage(Id);
public record ErrorDescription(int Code, string Message);
public record NotificationMessage(string Method, JToken? Params) : Message;

[Service(Lifetime.Singleton)]
public class MessageConverter
{
    private readonly JsonRpcSerializer _serializer;

    public MessageConverter(JsonRpcSerializer serializer)
    {
        _serializer = serializer;
    }

    public Message FromBytes(byte[] data)
    {
        var msg = _serializer.ByteArrayToObject<GenericMessage>(data);
        if (msg == null)
            throw new Exception("Could not parse message");

        if (!msg.Id.HasValue)
        {
            if (msg.Method == null)
                throw new Exception("Message is missing a method name");
            return new NotificationMessage(msg.Method, msg.Params);
        }

        if (msg.Method != null)
            return new RequestMessage(msg.Id.Value, msg.Method, msg.Params);

        if (msg.Error != null)
            return new ResponseErrorMessage(msg.Id.Value, msg.Error);

        return new ResponseSuccessMessage(msg.Id.Value, msg.Result);
    }

    private record GenericMessage(long? Id, string? Method, JToken? Params, JToken? Result, ErrorDescription? Error);
}