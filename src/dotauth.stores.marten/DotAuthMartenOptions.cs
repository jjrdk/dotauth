﻿namespace DotAuth.Stores.Marten;

using System;
using System.Data.Common;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using global::Marten;
using Newtonsoft.Json;
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

    private sealed class ClaimConverter : JsonConverter<Claim>
    {
        public override void WriteJson(JsonWriter writer, Claim? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                return;
            }

            var info = new ClaimInfo(value.Type, value.Value);
            serializer.Serialize(writer, info);
        }

        public override Claim ReadJson(
            JsonReader reader,
            Type objectType,
            Claim? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var info = serializer.Deserialize<ClaimInfo>(reader);
            return new Claim(info.Type, info.Value);
        }
    }

    private readonly struct ClaimInfo
    {
        public ClaimInfo(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; }

        public string Value { get; }
    }

    private sealed class CustomJsonSerializer : ISerializer
    {
        private readonly JsonSerializer _innerSerializer;

        public CustomJsonSerializer()
        {
            _innerSerializer = new JsonSerializer
            {
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                DefaultValueHandling = DefaultValueHandling.Include,
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double,
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.Auto,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };
            _innerSerializer.Converters.Add(new ClaimConverter());
        }

        public string ToJson(object? document)
        {
            if (document == null)
            {
                return "null";
            }
            var sb = new StringBuilder();
            using var writer = new StringWriter(sb);
            _innerSerializer.Serialize(writer, document, document.GetType());

            return sb.ToString();
        }

        /// <inheritdoc />
        public T FromJson<T>(Stream stream)
        {
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);
            return _innerSerializer.Deserialize<T>(reader)
                   ?? throw new NullReferenceException("Could not deserialize from stream");
        }

        /// <inheritdoc />
        public T FromJson<T>(DbDataReader reader, int index)
        {
            using var sr = new StringReader(reader.GetString(index));
            using var r = new JsonTextReader(sr);
            return _innerSerializer.Deserialize<T>(r)
                   ?? throw new NullReferenceException("Could not deserialize from DbDataReader");
        }

        /// <inheritdoc />
        public ValueTask<T> FromJsonAsync<T>(Stream stream, CancellationToken cancellationToken = new())
        {
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);
            var item = _innerSerializer.Deserialize<T>(reader)
                       ?? throw new NullReferenceException("Could not deserialize from stream");
            return new ValueTask<T>(item);
        }

        /// <inheritdoc />
        public ValueTask<T> FromJsonAsync<T>(
            DbDataReader reader,
            int index,
            CancellationToken cancellationToken = new())
        {
            using var sr = new StringReader(reader.GetString(index));
            using var r = new JsonTextReader(sr);
            var item = _innerSerializer.Deserialize<T>(r)
                       ?? throw new NullReferenceException("Could not deserialize from stream");
            return new ValueTask<T>(item);
        }

        /// <inheritdoc />
        public object FromJson(Type type, Stream stream)
        {
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);
            return _innerSerializer.Deserialize(reader, type)
                   ?? throw new NullReferenceException("Could not deserialize from stream");
        }

        /// <inheritdoc />
        public object FromJson(Type type, DbDataReader reader, int index)
        {
            using var sr = new StringReader(reader.GetString(index));
            using var r = new JsonTextReader(sr);
            return _innerSerializer.Deserialize(r, type)
                   ?? throw new NullReferenceException("Could not deserialize from DbDataReader");
        }

        /// <inheritdoc />
        public ValueTask<object> FromJsonAsync(Type type, Stream stream, CancellationToken cancellationToken = new())
        {
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);
            var item = _innerSerializer.Deserialize(reader, type)
                       ?? throw new NullReferenceException("Could not deserialize from stream");
            return new ValueTask<object>(item);
        }

        /// <inheritdoc />
        public ValueTask<object> FromJsonAsync(
            Type type,
            DbDataReader reader,
            int index,
            CancellationToken cancellationToken = new())
        {
            using var sr = new StringReader(reader.GetString(index));
            using var r = new JsonTextReader(sr);
            var item = _innerSerializer.Deserialize(r, type)
                       ?? throw new NullReferenceException("Could not deserialize from stream");
            return new ValueTask<object>(item);
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