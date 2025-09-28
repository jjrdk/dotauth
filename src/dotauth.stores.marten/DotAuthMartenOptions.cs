namespace DotAuth.Stores.Marten;

using System;
using System.Data.Common;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using global::Marten;
using JasperFx;
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
//
//    private sealed class ClaimConverter : JsonSerializerer<Claim>
//    {
//        public override void WriteJson(JsonWriter writer, Claim? value, JsonSerializer serializer)
//        {
//            if (value == null)
//            {
//                return;
//            }
//
//            var info = new ClaimInfo(value.Type, value.Value);
//            serializer.Serialize(writer, info);
//        }
//
//        public override Claim? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//        {
//            reader.Read()
//            return new Claim(info.Type, info.Value);
//        }
//
//        public override void Write(Utf8JsonWriter writer, Claim value, JsonSerializerOptions options)
//        {
//            throw new NotImplementedException();
//        }
//    }
//
//    private readonly struct ClaimInfo
//    {
//        public ClaimInfo(string type, string value)
//        {
//            Type = type;
//            Value = value;
//        }
//
//        public string Type { get; }
//
//        public string Value { get; }
//    }

    private sealed class CustomJsonSerializer : ISerializer
    {
        public string ToJson(object? document)
        {
            if (document == null)
            {
                return "null";
            }

            return JsonSerializer.Serialize(document, document.GetType(), SharedSerializerContext.Default);
        }

        /// <inheritdoc />
        public T FromJson<T>(Stream stream)
        {
            return JsonSerializer.Deserialize<T>(stream, DefaultJsonSerializerOptions.Instance) ??
                throw new NullReferenceException("Could not deserialize from stream");
        }

        /// <inheritdoc />
        public T FromJson<T>(DbDataReader reader, int index)
        {
            return JsonSerializer.Deserialize<T>(reader.GetString(index), DefaultJsonSerializerOptions.Instance)
             ?? throw new NullReferenceException("Could not deserialize from DbDataReader");
        }

        /// <inheritdoc />
        public async ValueTask<T> FromJsonAsync<T>(Stream stream, CancellationToken cancellationToken = new())
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, DefaultJsonSerializerOptions.Instance,
                    cancellationToken)
             ?? throw new NullReferenceException("Could not deserialize from stream");
        }

        /// <inheritdoc />
        public async ValueTask<T> FromJsonAsync<T>(
            DbDataReader reader,
            int index,
            CancellationToken cancellationToken = new())
        {
            return await JsonSerializer.DeserializeAsync<T>(reader.GetStream(index),
                    DefaultJsonSerializerOptions.Instance,
                    cancellationToken)
             ?? throw new NullReferenceException("Could not deserialize from stream");
        }

        /// <inheritdoc />
        public object FromJson(Type type, Stream stream)
        {
            return JsonSerializer.Deserialize(stream, type, DefaultJsonSerializerOptions.Instance)
             ?? throw new NullReferenceException("Could not deserialize from stream");
        }

        /// <inheritdoc />
        public object FromJson(Type type, DbDataReader reader, int index)
        {
            return JsonSerializer.Deserialize(reader.GetString(index), type)
             ?? throw new NullReferenceException("Could not deserialize from DbDataReader");
        }

        /// <inheritdoc />
        public async ValueTask<object> FromJsonAsync(
            Type type,
            Stream stream,
            CancellationToken cancellationToken = new())
        {
            return await JsonSerializer.DeserializeAsync(stream,
                    options: DefaultJsonSerializerOptions.Instance,
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
            return await JsonSerializer.DeserializeAsync(reader.GetStream(index), type, SharedSerializerContext.Default,
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
