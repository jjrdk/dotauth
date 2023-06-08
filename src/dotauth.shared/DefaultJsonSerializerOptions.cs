namespace DotAuth.Shared;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;

internal static class DefaultJsonSerializerOptions
{
    static DefaultJsonSerializerOptions()
    {
        Instance = new JsonSerializerOptions
        {
            TypeInfoResolver = DataContractResolver.Default,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            NumberHandling = JsonNumberHandling.AllowReadingFromString |
                JsonNumberHandling.AllowNamedFloatingPointLiterals,
            AllowTrailingCommas = true,
            Converters =
            {
                new ClaimConverter(),
                new JwtPayloadConverter(),
                new JsonWebKeySetConverter(),
                new RegexConverter()
            },
            ReadCommentHandling = JsonCommentHandling.Skip,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode
        };
    }

    public static JsonSerializerOptions Instance { get; }

    private sealed class RegexConverter : JsonConverter<Regex>
    {
        public override Regex Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var regex = reader.GetString();
            return new Regex(regex ?? "", RegexOptions.Compiled);
        }

        public override void Write(Utf8JsonWriter writer, Regex value, JsonSerializerOptions options)
        {
            writer.WriteRawValue($"\"{value}\"");
        }
    }

    private sealed class ClaimConverter : JsonConverter<Claim>
    {
        public override Claim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var obj = JsonSerializer.Deserialize<JsonObject>(ref reader, options);
            if (obj == null)
            {
                throw new Exception("Failed to read json");
            }

            var properties = obj.Select(x => x.Key).ToArray();
            if (properties.Length == 1)
            {
                var type = obj.First().Key;
                var value = obj[type];
                return new Claim(type, value?.GetValue<string>() ?? string.Empty);
            }

            return new Claim(
                obj["type"]!.GetValue<string>(),
                obj["value"]!.GetValue<string>(),
                obj["valueType"]?.GetValue<string>(),
                obj["issuer"]?.GetValue<string>());
        }

        public override void Write(Utf8JsonWriter writer, Claim value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString(value.Type, value.Value);
            writer.WriteEndObject();
        }
    }

    private sealed class JwtPayloadConverter : JsonConverter<JwtPayload>
    {
        public override JwtPayload Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var obj = JsonSerializer.Deserialize<JsonObject>(ref reader, options);
            var jsonNode = obj!["claims"]!.AsArray();
            return new JwtPayload(
                null,
                null,
                jsonNode.AsArray().Deserialize<Claim[]>(options),
                null,
                null);
        }

        public override void Write(Utf8JsonWriter writer, JwtPayload value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("acr", value.Acr);
            writer.WriteString("azp", value.Azp);
            writer.WriteStartArray("claims");
            foreach (var claim in value.Claims)
            {
                writer.WriteStartObject();
                writer.WriteString("type", claim.Type);
                writer.WriteString("value", claim.Value);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

    private sealed class JsonWebKeySetConverter : JsonConverter<JsonWebKeySet>
    {
        public override JsonWebKeySet Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var obj = JsonSerializer.Deserialize<JsonObject>(ref reader, options);
            var json = obj?.ToJsonString();
            return new JsonWebKeySet(json);
        }

        public override void Write(Utf8JsonWriter writer, JsonWebKeySet value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("keys");
            writer.WriteRawValue(JsonSerializer.Serialize(value.Keys, options));
            writer.WriteEndObject();
        }
    }
}
