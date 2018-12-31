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

namespace SimpleAuth.Api.Jws
{
    using Errors;
    using Exceptions;
    using Extensions;
    using Helpers;
    using Parameters;
    using Results;
    using Shared;
    using Signature;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    public class JwsActions
    {
        private readonly IJwsParser _jwsParser;
        private readonly IJwsGenerator _jwsGenerator;
        private readonly IJsonWebKeyHelper _jsonWebKeyHelper;
        private readonly JsonWebKeyEnricher _jsonWebKeyEnricher = new JsonWebKeyEnricher();

        public JwsActions(
            IJwsParser jwsParser,
            IJwsGenerator jwsGenerator,
            IJsonWebKeyHelper jsonWebKeyHelper)
        {
            _jwsParser = jwsParser;
            _jwsGenerator = jwsGenerator;
            _jsonWebKeyHelper = jsonWebKeyHelper;
        }

        public async Task<JwsInformationResult> GetJwsInformation(GetJwsParameter getJwsParameter)
        {
            if (getJwsParameter == null || string.IsNullOrWhiteSpace(getJwsParameter.Jws))
            {
                throw new ArgumentNullException(nameof(getJwsParameter));
            }

            if (getJwsParameter.Url != null && !getJwsParameter.Url.IsAbsoluteUri)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, getJwsParameter.Url));
            }

            var jws = getJwsParameter.Jws;
            var jwsHeader = _jwsParser.GetHeader(jws);
            if (jwsHeader == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheTokenIsNotAValidJws);
            }

            if (!string.Equals(jwsHeader.Alg, JwtConstants.JwsAlgNames.NONE, StringComparison.CurrentCultureIgnoreCase)
                && getJwsParameter.Url == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheSignatureCannotBeChecked);
            }

            var result = new JwsInformationResult
            {
                Header = jwsHeader
            };

            JwsPayload payload;
            if (!string.Equals(jwsHeader.Alg, JwtConstants.JwsAlgNames.NONE, StringComparison.CurrentCultureIgnoreCase))
            {
                var jsonWebKey = await _jsonWebKeyHelper.GetJsonWebKey(jwsHeader.Kid, getJwsParameter.Url)
                    .ConfigureAwait(false);
                if (jsonWebKey == null)
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(
                            ErrorDescriptions.TheJsonWebKeyCannotBeFound,
                            jwsHeader.Kid,
                            getJwsParameter.Url.AbsoluteUri));
                }

                payload = _jwsParser.ValidateSignature(jws, jsonWebKey);
                if (payload == null)
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidRequestCode,
                        ErrorDescriptions.TheSignatureIsNotCorrect);
                }

                var jsonWebKeyDic = _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey);
                jsonWebKeyDic.AddRange(_jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey));
                result.JsonWebKey = jsonWebKeyDic;
            }
            else
            {
                payload = _jwsParser.GetPayload(jws);
            }

            result.Payload = payload;
            return result;
        }

        public async Task<string> CreateJws(CreateJwsParameter createJwsParameter)
        {
            if (createJwsParameter == null)
            {
                throw new ArgumentNullException(nameof(createJwsParameter));
            }

            if (createJwsParameter.Payload == null
                || !createJwsParameter.Payload.Any())
            {
                throw new ArgumentNullException(nameof(createJwsParameter.Payload));
            }

            if (createJwsParameter.Alg != JwsAlg.none &&
                (string.IsNullOrWhiteSpace(createJwsParameter.Kid) || string.IsNullOrWhiteSpace(createJwsParameter.Url)))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheJwsCannotBeGeneratedBecauseMissingParameters);
            }

            Uri uri = null;
            if (createJwsParameter.Alg != JwsAlg.none && !Uri.TryCreate(createJwsParameter.Url, UriKind.Absolute, out uri))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheUrlIsNotWellFormed);
            }

            JsonWebKey jsonWebKey = null;
            if (createJwsParameter.Alg != JwsAlg.none)
            {
                jsonWebKey = await _jsonWebKeyHelper.GetJsonWebKey(createJwsParameter.Kid, uri).ConfigureAwait(false);
                if (jsonWebKey == null)
                {
                    throw new SimpleAuthException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, createJwsParameter.Kid, uri.AbsoluteUri));
                }
            }

            return _jwsGenerator.Generate(createJwsParameter.Payload,
                createJwsParameter.Alg,
                jsonWebKey);
        }

        private class JsonWebKeyEnricher
        {
            private readonly Dictionary<KeyType, Action<Dictionary<string, object>, JsonWebKey>> _mappingKeyTypeAndPublicKeyEnricher;

            public JsonWebKeyEnricher()
            {
                _mappingKeyTypeAndPublicKeyEnricher =
                    new Dictionary<KeyType, Action<Dictionary<string, object>, JsonWebKey>>
                    {
                        {
                            KeyType.RSA, SetRsaPublicKeyInformation
                        }
                    };
            }

            public Dictionary<string, object> GetPublicKeyInformation(JsonWebKey jsonWebKey)
            {
                if (jsonWebKey == null)
                {
                    throw new ArgumentNullException(nameof(jsonWebKey));
                }

                if (!_mappingKeyTypeAndPublicKeyEnricher.ContainsKey(jsonWebKey.Kty))
                {
                    throw new SimpleAuthException(ErrorCodes.InvalidParameterCode,
                        string.Format(ErrorDescriptions.TheKtyIsNotSupported, jsonWebKey.Kty));
                }

                var result = new Dictionary<string, object>();
                var enricher = _mappingKeyTypeAndPublicKeyEnricher[jsonWebKey.Kty];
                enricher(result, jsonWebKey);
                return result;
            }

            public Dictionary<string, object> GetJsonWebKeyInformation(JsonWebKey jsonWebKey)
            {
                if (!JwtConstants.MappingKeyTypeEnumToName.ContainsKey(jsonWebKey.Kty))
                {
                    throw new ArgumentException(nameof(jsonWebKey.Kty));
                }

                if (!JwtConstants.MappingUseEnumerationToName.ContainsKey(jsonWebKey.Use))
                {
                    throw new ArgumentException(nameof(jsonWebKey.Use));
                }

                return new Dictionary<string, object>
                {
                    {
                        JwtConstants.JsonWebKeyParameterNames.KeyTypeName,
                        JwtConstants.MappingKeyTypeEnumToName[jsonWebKey.Kty]
                    },
                    {
                        JwtConstants.JsonWebKeyParameterNames.UseName,
                        JwtConstants.MappingUseEnumerationToName[jsonWebKey.Use]
                    },
                    {
                        JwtConstants.JsonWebKeyParameterNames.AlgorithmName,
                        JwtConstants.MappingNameToAllAlgEnum.SingleOrDefault(kp => kp.Value == jsonWebKey.Alg).Key
                    },
                    {
                        JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName, jsonWebKey.Kid
                    }
                    // TODO : we still need to support the other parameters x5u & x5c & x5t & x5t#S256
                };
            }

            private void SetRsaPublicKeyInformation(Dictionary<string, object> result, JsonWebKey jsonWebKey)
            {
                RSAParameters rsaParameters;
                using (var provider = new RSACryptoServiceProvider())
                {
                    RsaExtensions.FromXmlString(provider, jsonWebKey.SerializedKey);
                    //provider.FromXmlString(jsonWebKey.SerializedKey);
                    //RsaExtensions.FromXmlString(provider, jsonWebKey.SerializedKey);
                    rsaParameters = provider.ExportParameters(false);
                }

                // Export the modulus
                var modulus = rsaParameters.Modulus.ToBase64Simplified();
                // Export the exponent
                var exponent = rsaParameters.Exponent.ToBase64Simplified();

                result.Add(JwtConstants.JsonWebKeyParameterNames.RsaKey.ModulusName, modulus);
                result.Add(JwtConstants.JsonWebKeyParameterNames.RsaKey.ExponentName, exponent);
            }
        }
    }
}
