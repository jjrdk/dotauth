namespace DotAuth.Stores.Marten;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Stores.Marten.Containers;
using global::Marten;
using JasperFx;
using JasperFx.Core.Reflection;
using Weasel.Core;

/// <summary>
/// Defines the marten options for DotAuth.
/// </summary>
public sealed class DotAuthMartenOptions : StoreOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DotAuthMartenOptions"/> class.
    /// </summary>
    /// <param name="connectionString">The connection string</param>
    /// <param name="logger">The logger.</param>
    /// <param name="searchPath">The schema name</param>
    /// <param name="autoCreate">Schema creation options</param>
    public DotAuthMartenOptions(
        string connectionString,
        IMartenLogger? logger = null,
        string searchPath = "",
        AutoCreate autoCreate = AutoCreate.CreateOrUpdate)
    {
        Serializer<CustomJsonSerializer>();
        Connection(connectionString);
        if (logger != null)
        {
            Logger(logger);
        }

        Schema.Include<DotAuthRegistry>();
        if (!string.IsNullOrWhiteSpace(searchPath))
        {
            DatabaseSchemaName = searchPath;
        }

        Policies.AllDocumentsAreMultiTenanted();
        AutoCreateSchemaObjects = autoCreate;
        Advanced.DuplicatedFieldEnumStorage = EnumStorage.AsString;
        Advanced.DuplicatedFieldUseTimestampWithoutTimeZoneForDateTime = true;
    }

    private sealed class CustomJsonSerializer : ISerializer
    {
        public string ToJson(object? document)
        {
            if (document is null)
            {
                return "null";
            }

            if (document is Dictionary<string, object> dict)
            {
                var jsonObj = new JsonObject();
                foreach (var (key, value) in dict)
                {
                    if (value is IList list)
                    {
                        var jsonArray = new JsonArray();
                        foreach (var item in list)
                        {
                            if (item is null)
                            {
                                jsonArray.Add(null);
                            }
                            else if (item.GetType().IsSimple())
                            {
                                jsonArray.Add(JsonValue.Create(item));
                            }
                            else
                            {
                                var serializedItem =
                                    JsonSerializer.SerializeToElement(item, MartenSerializerContext.Default.Options);
                                jsonArray.Add(serializedItem);
                            }
                        }

                        jsonObj[key] = jsonArray;
                    }
                    else if (value.GetType().IsSimple())
                    {
                        jsonObj[key] = JsonValue.Create(value);
                    }
                    else
                    {
                        var serializedValue =
                            JsonSerializer.SerializeToNode(value, MartenSerializerContext.Default.Options);
                        jsonObj[key] = serializedValue;
                    }
                }
                return jsonObj.ToJsonString(MartenSerializerContext.Default.Options);
            }

            return JsonSerializer.Serialize(document, document.GetType(), MartenSerializerContext.Default);
        }

        /// <inheritdoc />
        public T FromJson<T>(Stream stream)
        {
            return JsonSerializer.Deserialize<T>(stream, MartenSerializerContext.Default.Options) ??
                throw new NullReferenceException("Could not deserialize from stream");
        }

        /// <inheritdoc />
        public T FromJson<T>(DbDataReader reader, int index)
        {
            return JsonSerializer.Deserialize<T>(reader.GetString(index), MartenSerializerContext.Default.Options)
             ?? throw new NullReferenceException("Could not deserialize from DbDataReader");
        }

        /// <inheritdoc />
        public async ValueTask<T> FromJsonAsync<T>(Stream stream, CancellationToken cancellationToken = new())
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, MartenSerializerContext.Default.Options,
                    cancellationToken)
             ?? throw new NullReferenceException("Could not deserialize from stream");
        }

        /// <inheritdoc />
        public async ValueTask<T> FromJsonAsync<T>(
            DbDataReader reader,
            int index,
            CancellationToken cancellationToken = new())
        {
            await using var stream = reader.GetStream(index);
            using var sr = new StreamReader(stream, Encoding.UTF8);
            var json = await sr.ReadToEndAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<T>(
                    json.Trim((char)1),
                    MartenSerializerContext.Default.Options)
             ?? throw new NullReferenceException("Could not deserialize from stream");
            return result;
        }

        /// <inheritdoc />
        public object FromJson(Type type, Stream stream)
        {
            return JsonSerializer.Deserialize(stream, type, MartenSerializerContext.Default)
             ?? throw new NullReferenceException("Could not deserialize from stream");
        }

        /// <inheritdoc />
        public object FromJson(Type type, DbDataReader reader, int index)
        {
            return JsonSerializer.Deserialize(reader.GetString(index), type, MartenSerializerContext.Default)
             ?? throw new NullReferenceException("Could not deserialize from DbDataReader");
        }

        /// <inheritdoc />
        public async ValueTask<object> FromJsonAsync(
            Type type,
            Stream stream,
            CancellationToken cancellationToken = new())
        {
            return await JsonSerializer.DeserializeAsync(stream,
                    options: MartenSerializerContext.Default.Options,
                    returnType: type,
                    cancellationToken: cancellationToken)
             ?? throw new NullReferenceException("Could not deserialize from stream");
        }

        /// <inheritdoc />
        public async ValueTask<object> FromJsonAsync(
            Type type,
            DbDataReader reader,
            int index,
            CancellationToken cancellationToken = new())
        {
            return await JsonSerializer.DeserializeAsync(reader.GetStream(index),
                    type,
                    MartenSerializerContext.Default,
                    cancellationToken)
             ?? throw new NullReferenceException("Could not deserialize from stream");
        }

        public string ToCleanJson(object? document)
        {
            return document == null ? "null" : ToJson(document);
        }

        /// <inheritdoc />
        public string ToJsonWithTypes(object document)
        {
            return ToJson(document);
        }

        public EnumStorage EnumStorage
        {
            get { return EnumStorage.AsString; }
        }

        public Casing Casing
        {
            get { return Casing.CamelCase; }
        }

        /// <inheritdoc />
        public ValueCasting ValueCasting
        {
            get { return ValueCasting.Strict; }
        }
    }
}

/// <inheritdoc cref="System.Text.Json.Serialization.JsonSerializerContext" />
[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString |
        JsonNumberHandling.AllowNamedFloatingPointLiterals,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
    UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
    Converters =
    [
        typeof(ClaimConverter),
        typeof(JwtPayloadConverter),
        typeof(JsonWebKeySetConverter),
        typeof(RegexConverter),
    ])]
[JsonSerializable(typeof(Client))]
[JsonSerializable(typeof(ClientSecret))]
[JsonSerializable(typeof(ClaimData))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(ErrorDetails))]
[JsonSerializable(typeof(GrantedToken))]
[JsonSerializable(typeof(GrantedToken[]))]
[JsonSerializable(typeof(Permission))]
[JsonSerializable(typeof(Permission[]))]
[JsonSerializable(typeof(PolicyRule))]
[JsonSerializable(typeof(ResourceOwner))]
[JsonSerializable(typeof(ResourceSet))]
[JsonSerializable(typeof(OwnedResourceSet))]
[JsonSerializable(typeof(ResourceSetDescription))]
[JsonSerializable(typeof(Consent))]
[JsonSerializable(typeof(ScopeContainer))]
[JsonSerializable(typeof(Ticket))]
[JsonSerializable(typeof(TicketLine))]
public partial class MartenSerializerContext : JsonSerializerContext
{
}
