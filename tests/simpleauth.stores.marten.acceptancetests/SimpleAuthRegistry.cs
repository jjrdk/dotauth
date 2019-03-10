namespace SimpleAuth.Stores.Marten.AcceptanceTests
{
    using global::Marten;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using SimpleAuth.Shared.AccountFiltering;
    using SimpleAuth.Shared.Models;
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;

    internal class SimpleAuthMartenOptions : StoreOptions
    {
        public SimpleAuthMartenOptions(string connectionString, string searchPath)
        {
            Serializer<CustomJsonSerializer>();
            Connection(connectionString);
            Schema.Include<SimpleAuthRegistry>();
            DatabaseSchemaName = searchPath;
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

        private class CustomJsonSerializer : ISerializer
        {
            private readonly JsonSerializer _innerSerializer;

            public CustomJsonSerializer()
            {
                _innerSerializer = new JsonSerializer
                {
                    MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    StringEscapeHandling = StringEscapeHandling.EscapeHtml
                };
                _innerSerializer.Converters.Add(new ClaimConverter());
            }

            public void ToJson(object document, TextWriter writer)
            {
                _innerSerializer.Serialize(writer, document);
            }

            public string ToJson(object document)
            {
                var sb = new StringBuilder();
                using (var writer = new StringWriter(sb))
                {
                    _innerSerializer.Serialize(writer, document, document.GetType());
                }

                return sb.ToString();
            }

            public T FromJson<T>(TextReader reader)
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return _innerSerializer.Deserialize<T>(jsonReader);
                }
            }

            public object FromJson(Type type, TextReader reader)
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return _innerSerializer.Deserialize(jsonReader, type);
                }
            }

            public string ToCleanJson(object document)
            {
                return ToJson(document);
            }

            public EnumStorage EnumStorage { get; } = EnumStorage.AsString;
            public Casing Casing { get; } = Casing.CamelCase;
        }
    }

    public class SimpleAuthRegistry : MartenRegistry
    {
        public SimpleAuthRegistry()
        {
            For<Scope>().Identity(x => x.Name).GinIndexJsonData();
            For<Filter>().Identity(x => x.Name).GinIndexJsonData();
            For<ResourceOwner>().Identity(x => x.Subject).GinIndexJsonData();
            For<Consent>().GinIndexJsonData();
            For<Policy>().GinIndexJsonData();
            For<Client>().Identity(x => x.ClientId).GinIndexJsonData();
            For<GrantedToken>().GinIndexJsonData();
        }
    }
}