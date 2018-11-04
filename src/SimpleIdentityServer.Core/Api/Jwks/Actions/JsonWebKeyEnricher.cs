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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace SimpleIdentityServer.Core.Api.Jwks.Actions
{
    using Jwt.Extensions;
    using Shared;

    public class JsonWebKeyEnricher : IJsonWebKeyEnricher
    {
        private readonly Dictionary<KeyType, Action<Dictionary<string, object>, JsonWebKey>> _mappingKeyTypeAndPublicKeyEnricher;

        public JsonWebKeyEnricher()
        {
            _mappingKeyTypeAndPublicKeyEnricher = new Dictionary<KeyType, Action<Dictionary<string, object>, JsonWebKey>>
            {
                {
                    KeyType.RSA, SetRsaPublicKeyInformation
                }
            };
        }
        
        public Dictionary<string, object> GetPublicKeyInformation(JsonWebKey jsonWebKey)
        {
            var result = new Dictionary<string, object>();
            var enricher = _mappingKeyTypeAndPublicKeyEnricher[jsonWebKey.Kty];
            enricher(result, jsonWebKey);
            return result;
        }

        public Dictionary<string, object> GetJsonWebKeyInformation(JsonWebKey jsonWebKey)
        {
            return new Dictionary<string, object>
            {
                {
                    Jwt.JwtConstants.JsonWebKeyParameterNames.KeyTypeName, Jwt.JwtConstants.MappingKeyTypeEnumToName[jsonWebKey.Kty]
                },
                {
                    Jwt.JwtConstants.JsonWebKeyParameterNames.UseName, Jwt.JwtConstants.MappingUseEnumerationToName[jsonWebKey.Use]
                },
                {
                    Jwt.JwtConstants.JsonWebKeyParameterNames.AlgorithmName, Jwt.JwtConstants.MappingNameToAllAlgEnum.SingleOrDefault(kp => kp.Value == jsonWebKey.Alg).Key
                },
                {
                    Jwt.JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName, jsonWebKey.Kid
                }
                // TODO : we still need to support the other parameters x5u & x5c & x5t & x5t#S256
            };
        }

        public void SetRsaPublicKeyInformation(Dictionary<string, object> result, JsonWebKey jsonWebKey)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (var provider = new RSACryptoServiceProvider())
                {
                    provider.FromXmlStringNetCore(jsonWebKey.SerializedKey);
                    var rsaParameters = provider.ExportParameters(false);
                    // Export the modulus
                    var modulus = rsaParameters.Modulus.ToBase64Simplified();
                    // Export the exponent
                    var exponent = rsaParameters.Exponent.ToBase64Simplified();

                    result.Add(Jwt.JwtConstants.JsonWebKeyParameterNames.RsaKey.ModulusName, modulus);
                    result.Add(Jwt.JwtConstants.JsonWebKeyParameterNames.RsaKey.ExponentName, exponent);
                }
            }
            else
            {
                using (var provider = new RSAOpenSsl())
                {
                    provider.FromXmlStringNetCore(jsonWebKey.SerializedKey);
                    var rsaParameters = provider.ExportParameters(false);
                    // Export the modulus
                    var modulus = rsaParameters.Modulus.ToBase64Simplified();
                    // Export the exponent
                    var exponent = rsaParameters.Exponent.ToBase64Simplified();

                    result.Add(Jwt.JwtConstants.JsonWebKeyParameterNames.RsaKey.ModulusName, modulus);
                    result.Add(Jwt.JwtConstants.JsonWebKeyParameterNames.RsaKey.ExponentName, exponent);
                }
            }
        }
    }
}
