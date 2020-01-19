namespace SimpleAuth.Client
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal sealed class Serializer
    {
        private static Serializer _inner;
        private readonly JsonSerializer _serializer;

        private Serializer()
        {
            _serializer = new JsonSerializer
            {
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };
            _serializer.Converters.Add(new ClaimConverter());
        }

        public static Serializer Default => _inner ?? (_inner = new Serializer());

        public string Serialize<T>(T item)
        {
            using var writer = new StringWriter();
            _serializer.Serialize(writer, item, typeof(T));
            writer.Flush();
            return writer.GetStringBuilder().ToString();
        }

        public T Deserialize<T>(string json)
        {
            using var reader = new StringReader(json);
            using var jsonReader = new JsonTextReader(reader);
            return _serializer.Deserialize<T>(jsonReader);
        }

        private class ClaimConverter : JsonConverter<Claim>
        {
            public override void WriteJson(JsonWriter writer, Claim value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(value.Type);
                writer.WriteValue(value.Value);
                writer.WriteEndObject();
            }

            public override Claim ReadJson(
                JsonReader reader,
                Type objectType,
                Claim existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
            {
                var obj = serializer.Deserialize<JObject>(reader);
                var properties = obj.Properties().ToArray();
                if (properties.Length == 1)
                {
                    var type = obj.Properties().First().Name;
                    var value = obj[type];
                    return new Claim(type, value.ToObject<string>());
                }

                return new Claim(
                    obj["type"].ToObject<string>(),
                    obj["value"].ToObject<string>(),
                    obj["valueType"].ToObject<string>(),
                    obj["issuer"].ToObject<string>());
            }
        }
    }
}