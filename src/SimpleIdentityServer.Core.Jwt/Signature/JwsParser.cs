// Copyright 2015 Habart Thierry
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

using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.DTOs.Requests;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt.Converter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Core.Jwt.Signature
{
    using Newtonsoft.Json;

    public class JwsParser : IJwsParser
    {
        private readonly ICreateJwsSignature _createJwsSignature;
        private readonly IJsonWebKeyConverter _jsonWebKeyConverter;
        private readonly JwsPayloadConverter _jwsPayloadConverter;

        public JwsParser(ICreateJwsSignature createJwsSignature)
        {
            _createJwsSignature = createJwsSignature;
            _jsonWebKeyConverter = new JsonWebKeyConverter();
            _jwsPayloadConverter = new JwsPayloadConverter();
        }

        /// <summary>
        /// Validate the signature and returns the JWSPayLoad.
        /// </summary>
        /// <param name="jws"></param>
        /// <param name="jsonWebKey"></param>
        /// <returns></returns>
        public JwsPayload ValidateSignature(string jws, JsonWebKey jsonWebKey)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                throw new ArgumentNullException(nameof(jws));
            }

            if (jsonWebKey == null)
            {
                throw new ArgumentNullException(nameof(jsonWebKey));
            }

            var parts = GetParts(jws);
            if (!parts.Any())
            {
                return null;
            }

            var base64EncodedProtectedHeader = parts[0];
            var base64EncodedSerialized = parts[1];
            var combinedProtectedHeaderAndPayLoad = string.Format("{0}.{1}", base64EncodedProtectedHeader,
                base64EncodedSerialized);
            var serializedProtectedHeader = base64EncodedProtectedHeader.Base64Decode();
            var serializedPayload = base64EncodedSerialized.Base64Decode();
            var signature = parts[2].Base64DecodeBytes();
            var protectedHeader = serializedProtectedHeader.DeserializeWithJavascript<JwsProtectedHeader>();
            if (!Enum.TryParse(protectedHeader.Alg, out JwsAlg jwsAlg))
            {
                return null;
            }

            var signatureIsCorrect = false;
            switch (jsonWebKey.Kty)
            {
                case KeyType.RSA:
                    // To validate we need the parameters : modulus & exponent.
                    signatureIsCorrect = _createJwsSignature.VerifyWithRsa(
                        jwsAlg,
                        jsonWebKey.SerializedKey,
                        combinedProtectedHeaderAndPayLoad,
                        signature);
                    break;
            }

            if (!signatureIsCorrect)
            {
                return null;
            }

            return serializedPayload.DeserializeWithJavascript<JwsPayload>(_jwsPayloadConverter);
        }

        /// <summary>
        /// Validate the signature and returns the JWSPayload.
        /// </summary>
        /// <param name="jws"></param>
        /// <param name="jsonWebKeySet"></param>
        /// <returns></returns>
        public JwsPayload ValidateSignature(string jws, JsonWebKeySet jsonWebKeySet)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                throw new ArgumentNullException(nameof(jws));
            }

            if (jsonWebKeySet == null)
            {
                throw new ArgumentNullException(nameof(jsonWebKeySet));
            }

            if (jsonWebKeySet.Keys == null)
            {
                throw new ArgumentNullException(nameof(jsonWebKeySet.Keys));
            }

            var jsonWebKeys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
            if (!jsonWebKeys.Any())
            {
                return null;
            }

            var header = GetHeader(jws);
            var jsonWebKey = jsonWebKeys.FirstOrDefault(s => s.Kid == header.Kid);
            if (jsonWebKey == null)
            {
                return null;
            }

            return ValidateSignature(jws, jsonWebKey);
        }

        public JwsProtectedHeader GetHeader(string jws)
        {
            var parts = GetParts(jws);
            if (!parts.Any())
            {
                return null;
            }

            var base64EncodedProtectedHeader = parts[0];
            var serializedProtectedHeader = base64EncodedProtectedHeader.Base64Decode();
            return serializedProtectedHeader.DeserializeWithJavascript<JwsProtectedHeader>();
        }

        public JwsPayload GetPayload(string jws)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                throw new ArgumentNullException("jws");
            }

            var parts = GetParts(jws);
            if (!parts.Any())
            {
                return null;
            }

            var base64EncodedSerialized = parts[1];
            var serializedPayload = base64EncodedSerialized.Base64Decode();
            return serializedPayload.DeserializeWithJavascript<JwsPayload>(_jwsPayloadConverter);
        }

        /// <summary>
        /// Split the JWS into three parts.
        /// </summary>
        /// <param name="jws"></param>
        /// <returns></returns>
        private static List<string> GetParts(string jws)
        {
            if (string.IsNullOrWhiteSpace(jws))
            {
                return null;
            }

            var parts = jws.Split('.');
            return parts.Length != 3 ? new List<string>() : parts.ToList();
        }
    }

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
