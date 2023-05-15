// Code copied from https://github.com/zcsizmadia/ZCS.DataContractResolver

namespace DotAuth.Shared;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading;
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
            var jsonNode = obj["claims"].AsArray();
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

internal class DataContractResolver : IJsonTypeInfoResolver
{
    private static DataContractResolver? _defaultInstance;

    public static DataContractResolver Default
    {
        get
        {
            if (_defaultInstance is { } result)
            {
                return result;
            }

            DataContractResolver newInstance = new();
            var originalInstance =
                Interlocked.CompareExchange(ref _defaultInstance, newInstance, comparand: null);
            return originalInstance ?? newInstance;
        }
    }

    private static bool IsNullOrDefault(object? obj)
    {
        if (obj is null)
        {
            return true;
        }

        var type = obj.GetType();

        return type.IsValueType && FormatterServices.GetUninitializedObject(type).Equals(obj);
    }

    private static IEnumerable<MemberInfo> EnumerateFieldsAndProperties(Type type, BindingFlags bindingFlags)
    {
        foreach (var fieldInfo in type.GetFields(bindingFlags))
        {
            yield return fieldInfo;
        }

        foreach (var propertyInfo in type.GetProperties(bindingFlags))
        {
            yield return propertyInfo;
        }
    }

    private static IEnumerable<JsonPropertyInfo> CreateDataMembers(JsonTypeInfo jsonTypeInfo)
    {
        var isDataContract = jsonTypeInfo.Type.GetCustomAttribute<DataContractAttribute>() != null;
        var bindingFlags = BindingFlags.Instance | BindingFlags.Public;

        if (isDataContract)
        {
            bindingFlags |= BindingFlags.NonPublic;
        }

        foreach (var memberInfo in EnumerateFieldsAndProperties(jsonTypeInfo.Type, bindingFlags))
        {
            DataMemberAttribute? attr = null;
            if (isDataContract)
            {
                attr = memberInfo.GetCustomAttribute<DataMemberAttribute>();
                if (attr == null)
                {
                    continue;
                }
            }
            else
            {
                if (memberInfo.GetCustomAttribute<IgnoreDataMemberAttribute>() != null)
                {
                    continue;
                }
            }

            Func<object, object?>? getValue = null;
            Action<object, object?>? setValue = null;
            Type? propertyType;
            string? propertyName;

            if (memberInfo.MemberType == MemberTypes.Field && memberInfo is FieldInfo fieldInfo)
            {
                propertyName = attr?.Name ?? fieldInfo.Name;
                propertyType = fieldInfo.FieldType;
                getValue = fieldInfo.GetValue;
                setValue = (obj, value) => fieldInfo.SetValue(obj, value);
            }
            else if (memberInfo.MemberType == MemberTypes.Property && memberInfo is PropertyInfo propertyInfo)
            {
                propertyName = attr?.Name ?? propertyInfo.Name;
                propertyType = propertyInfo.PropertyType;
                if (propertyInfo.CanRead)
                {
                    getValue = propertyInfo.GetValue;
                }

                if (propertyInfo.CanWrite)
                {
                    setValue = (obj, value) => propertyInfo.SetValue(obj, value);
                }
            }
            else
            {
                continue;
            }

            var jsonPropertyInfo = jsonTypeInfo.CreateJsonPropertyInfo(propertyType, propertyName);

            jsonPropertyInfo.Get = getValue;
            jsonPropertyInfo.Set = setValue;

            if (attr != null)
            {
                jsonPropertyInfo.Order = attr.Order;
                jsonPropertyInfo.ShouldSerialize = !attr.EmitDefaultValue ? ((_, obj) => !IsNullOrDefault(obj)) : null;
            }

            yield return jsonPropertyInfo;
        }
    }

    public static JsonTypeInfo GetTypeInfo(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object)
        {
            jsonTypeInfo.CreateObject = () => Activator.CreateInstance(jsonTypeInfo.Type)!;

            foreach (var jsonPropertyInfo in CreateDataMembers(jsonTypeInfo).OrderBy((x) => x.Order))
            {
                jsonTypeInfo.Properties.Add(jsonPropertyInfo);
            }
        }

        return jsonTypeInfo;
    }

    public JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = JsonTypeInfo.CreateJsonTypeInfo(type, options);

        return GetTypeInfo(jsonTypeInfo);
    }
}
