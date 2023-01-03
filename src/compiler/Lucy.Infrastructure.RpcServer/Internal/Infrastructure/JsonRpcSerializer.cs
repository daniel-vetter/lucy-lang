using System.Text;
using Lucy.Common.ServiceDiscovery;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

[Service(Lifetime.Singleton)]
public class JsonRpcSerializer
{
    private readonly JsonSerializer _jsonSerializer;
    private readonly JsonSerializerSettings _settings;

    public JsonRpcSerializer(JsonRpcConfig config)
    {
        _settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Converters = config.JsonConverter
        };

        _jsonSerializer = JsonSerializer.Create(_settings);
    }

    public JToken? ObjectToToken(object? obj)
    {
        return obj == null ? JValue.CreateNull() : JToken.FromObject(obj, _jsonSerializer);
    }

    public string ObjectToString(object? obj)
    {
        return JsonConvert.SerializeObject(obj, _settings);
    }

    public byte[] ObjectToBytes(object? obj)
    {
        return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, _settings));
    }

    public T? ByteArrayToObject<T>(byte[] data)
    {
        var str = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(str, _settings);
    }

    public object? TokenToObject(JToken? token, System.Type type)
    {
        return token?.ToObject(type, _jsonSerializer);
    }
}