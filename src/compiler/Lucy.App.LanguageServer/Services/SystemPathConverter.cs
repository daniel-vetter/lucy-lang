using Lucy.Feature.LanguageServer.Models;
using Newtonsoft.Json;
using System;

namespace Lucy.Feature.LanguageServer.Services
{
    internal class SystemPathConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SystemPath);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.String)
                throw new Exception("Unexpected token type: " + reader.TokenType);

            return SystemPath.FromUri(new Uri((string)(reader.Value ?? throw new Exception("Could not parse json. TokenType was string but Value did not contain a string."))));
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteToken(JsonToken.String, value?.ToString());
        }
    }
}