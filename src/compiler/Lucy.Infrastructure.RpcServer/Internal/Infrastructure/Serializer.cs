using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Lucy.Infrastructure.RpcServer.Internal.Infrastructure
{
    public static class Serializer
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

        private static JsonSerializer JsonSerializer = JsonSerializer.Create(Settings);

        public static JToken? ObjectToToken(object? obj)
        {
            if (obj == null)
                return null;

            return JToken.FromObject(obj, JsonSerializer);
        }

        public static string ObjectToString(object? obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public static byte[] ObjectToBytes(object? obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, Settings));
        }

        public static T? ByteArrayToObject<T>(byte[] data)
        {
            var str = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(str, Settings);
        }

        internal static object? TokenToObject(JToken? token, System.Type type)
        {
            if (token == null)
                return null;
            return token.ToObject(type, JsonSerializer);
        }
    }
}
