// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth
{
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;

    public sealed class Serializer
    {
        private static Serializer _inner;
        private readonly JsonSerializer _serializer;

        private Serializer()
        {
            _serializer = new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };
            _serializer.Converters.Add(new ClaimConverter());
        }

        public static Serializer Default => _inner ?? (_inner = new Serializer());

        public string Serialize<T>(T item)
        {
            using (var writer = new StringWriter())
            {
                _serializer.Serialize(writer, item, typeof(T));
                writer.Flush();
                return writer.GetStringBuilder().ToString();
            }
        }

        public T Deserialize<T>(string json)
        {
            using (var reader = new StringReader(json))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return _serializer.Deserialize<T>(jsonReader);
                }
            }
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

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultTokenStore(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddSingleton<IAuthorizationCodeStore>(new InMemoryAuthorizationCodeStore());
            serviceCollection.AddSingleton<ITokenStore>(new InMemoryTokenStore());
            serviceCollection.AddSingleton<IConfirmationCodeStore>(new InMemoryConfirmationCode());
            return serviceCollection;
        }
    }
}
