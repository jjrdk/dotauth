namespace SimpleIdentityServer.Core.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class JwsPayloadConverter : JsonConverter<JwsPayload>
    {
        public override void WriteJson(JsonWriter writer, JwsPayload value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (var item in value)
            {
                writer.WritePropertyName(item.Key);
                serializer.Serialize(writer, item.Value);
            }
            writer.WriteEndObject();
        }

        public override JwsPayload ReadJson(
            JsonReader reader,
            Type objectType,
            JwsPayload existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var items = new List<KeyValuePair<string, object>>();
            string key = null;
            List<object> listItems = null;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.None:
                        break;
                    case JsonToken.StartObject:
                        break;
                    case JsonToken.StartArray:
                        listItems = new List<object>();
                        break;
                    case JsonToken.EndArray:
                        items.Add(new KeyValuePair<string, object>(key, listItems.ToArray()));
                        key = null;
                        listItems = null;
                        break;
                    case JsonToken.PropertyName:
                        key = reader.Value.ToString();
                        break;
                    case JsonToken.Raw:
                    case JsonToken.Undefined:
                    case JsonToken.Comment:
                        break;
                    case JsonToken.Null:
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.String:
                    case JsonToken.Date:
                    case JsonToken.Boolean:
                        if (listItems == null)
                        {
                            items.Add(new KeyValuePair<string, object>(key, reader.Value));
                            key = null;
                        }
                        else
                        {
                            listItems.Add(reader.Value);
                        }

                        break;
                    case JsonToken.EndObject:
                        break;
                    case JsonToken.StartConstructor:
                        break;
                    case JsonToken.EndConstructor:
                        break;
                    case JsonToken.Bytes:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var payload = new JwsPayload();
            payload.AddRange(items);
            return payload;
        }
    }
}