//// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
////     http://www.apache.org/licenses/LICENSE-2.0
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.

//namespace SimpleAuth.Api.Jwks
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Runtime.InteropServices;
//    using System.Security.Cryptography;
//    using System.Threading.Tasks;
//    using Extensions;
//    using Microsoft.IdentityModel.Tokens;
//    using Shared;
//    using Shared.Repositories;
//    using Shared.Requests;

//    public class JwksActions : IJwksActions
//    {
//        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;
//        private readonly ITokenStore _tokenStore;
//        private readonly JsonWebKeyEnricher _jsonWebKeyEnricher = new JsonWebKeyEnricher();

//        public JwksActions(
//            IJsonWebKeyRepository jsonWebKeyRepository, 
//            ITokenStore tokenStore)
//        {
//            _jsonWebKeyRepository = jsonWebKeyRepository;
//            _tokenStore = tokenStore;
//        }

//        public async Task<JsonWebKeySet> GetJwks()
//        {
//            var publicKeysUsedToValidateSignature = await GetSetOfPublicKeysUsedToValidateJwsAction().ConfigureAwait(false);
//            var publicKeysUsedForClientEncryption = await GetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction().ConfigureAwait(false);
//            var result = new JsonWebKeySet
//            {
//                Keys = new List<Dictionary<string, object>>()
//            };

//            result.Keys.AddRange(publicKeysUsedToValidateSignature);
//            result.Keys.AddRange(publicKeysUsedForClientEncryption);
//            return result;
//        }

//        public async Task<bool> RotateJwks()
//        {
//            var jsonWebKeys = await _jsonWebKeyRepository.GetAllAsync().ConfigureAwait(false);
//            if (jsonWebKeys == null || !jsonWebKeys.Keys.Any())
//            {
//                return false;
//            }

//            foreach (var jsonWebKey in jsonWebKeys.Keys)
//            {
//                string serializedRsa;
//                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//                {
//                    using (var provider = new RSACryptoServiceProvider())
//                    {
//                        serializedRsa = RsaExtensions.ToXmlString(provider, true);
//                    }
//                }
//                else
//                {
//                    using (var rsa = new RSAOpenSsl())
//                    {
//                        serializedRsa = RsaExtensions.ToXmlString(rsa, true);
//                    }
//                }

//                jsonWebKey.SerializedKey = serializedRsa;
//                await _jsonWebKeyRepository.UpdateAsync(jsonWebKey).ConfigureAwait(false);
//            }

//            await _tokenStore.Clean().ConfigureAwait(false);
//            return true;
//        }

//        private async Task<List<Dictionary<string, object>>> GetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction()
//        {
//            var result = new List<Dictionary<string, object>>();
//            var jsonWebKeys = await _jsonWebKeyRepository.GetAllAsync().ConfigureAwait(false);
//            // Retrieve all the JWK used by the client to encrypt the JWS
//            var jsonWebKeysUsedForEncryption =
//                jsonWebKeys.Keys.Where(jwk =>
//                    jwk.Use == JsonWebKeyUseNames.Enc && jwk.KeyOps.Contains(KeyOperations.Encrypt));
//            foreach (var jsonWebKey in jsonWebKeysUsedForEncryption)
//            {
//                var publicKeyInformation = _jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey);
//                var jsonWebKeyInformation = _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey);
//                // jsonWebKeyInformation.Add(Jwt.JwtConstants.JsonWebKeyParameterNames.KeyOperationsName, new List<string> { Jwt.JwtConstants.MappingKeyOperationToName[KeyOperations.Encrypt] } );
//                publicKeyInformation.AddRange(jsonWebKeyInformation);
//                result.Add(publicKeyInformation);
//            }

//            return result;
//        }

//        private async Task<List<Dictionary<string, object>>> GetSetOfPublicKeysUsedToValidateJwsAction()
//        {
//            var result = new List<Dictionary<string, object>>();
//            var jsonWebKeys = await _jsonWebKeyRepository.GetAllAsync().ConfigureAwait(false);
//            // Retrieve all the JWK used by the client to check the signature.
//            var jsonWebKeysUsedForSignature = jsonWebKeys.Keys.Where(jwk => jwk.Use == JsonWebKeyUseNames.Sig && jwk.KeyOps.Contains(KeyOperations.Verify));
//            foreach (var jsonWebKey in jsonWebKeysUsedForSignature)
//            {
//                var publicKeyInformation = _jsonWebKeyEnricher.GetPublicKeyInformation(jsonWebKey);
//                var jsonWebKeyInformation = _jsonWebKeyEnricher.GetJsonWebKeyInformation(jsonWebKey);
//                // jsonWebKeyInformation.Add(Jwt.JwtConstants.JsonWebKeyParameterNames.KeyOperationsName, new List<string> { Jwt.JwtConstants.MappingKeyOperationToName[KeyOperations.Verify] });
//                publicKeyInformation.AddRange(jsonWebKeyInformation);
//                result.Add(publicKeyInformation);
//            }

//            return result;
//        }
        
//        private class JsonWebKeyEnricher
//        {
//            private readonly Dictionary<KeyType, Action<Dictionary<string, object>, JsonWebKey>> _mappingKeyTypeAndPublicKeyEnricher;

//            public JsonWebKeyEnricher()
//            {
//                _mappingKeyTypeAndPublicKeyEnricher =
//                    new Dictionary<KeyType, Action<Dictionary<string, object>, JsonWebKey>>
//                    {
//                        {
//                            KeyType.RSA, SetRsaPublicKeyInformation
//                        }
//                    };
//            }

//            public Dictionary<string, object> GetPublicKeyInformation(JsonWebKey jsonWebKey)
//            {
//                var result = new Dictionary<string, object>();
//                var enricher = _mappingKeyTypeAndPublicKeyEnricher[jsonWebKey.Kty];
//                enricher(result, jsonWebKey);
//                return result;
//            }

//            public Dictionary<string, object> GetJsonWebKeyInformation(JsonWebKey jsonWebKey)
//            {
//                return new Dictionary<string, object>
//            {
//                {
//                    JwtConstants.JsonWebKeyParameterNames.KeyTypeName, JwtConstants.MappingKeyTypeEnumToName[jsonWebKey.Kty]
//                },
//                {
//                    JwtConstants.JsonWebKeyParameterNames.UseName, JwtConstants.MappingUseEnumerationToName[jsonWebKey.Use]
//                },
//                {
//                    JwtConstants.JsonWebKeyParameterNames.AlgorithmName, JwtConstants.MappingNameToAllAlgEnum.SingleOrDefault(kp => kp.Value == jsonWebKey.Alg).Key
//                },
//                {
//                    JwtConstants.JsonWebKeyParameterNames.KeyIdentifierName, jsonWebKey.Kid
//                }
//                // TODO : we still need to support the other parameters x5u & x5c & x5t & x5t#S256
//            };
//            }

//            public void SetRsaPublicKeyInformation(Dictionary<string, object> result, JsonWebKey jsonWebKey)
//            {
//                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//                {
//                    using (var provider = new RSACryptoServiceProvider())
//                    {
//                        RsaExtensions.FromXmlString(provider, jsonWebKey.SerializedKey);
//                        var rsaParameters = provider.ExportParameters(false);
//                        // Export the modulus
//                        var modulus = rsaParameters.Modulus.ToBase64Simplified();
//                        // Export the exponent
//                        var exponent = rsaParameters.Exponent.ToBase64Simplified();

//                        result.Add(JwtConstants.JsonWebKeyParameterNames.RsaKey.ModulusName, modulus);
//                        result.Add(JwtConstants.JsonWebKeyParameterNames.RsaKey.ExponentName, exponent);
//                    }
//                }
//                else
//                {
//                    using (var provider = new RSAOpenSsl())
//                    {
//                        RsaExtensions.FromXmlString(provider, jsonWebKey.SerializedKey);
//                        var rsaParameters = provider.ExportParameters(false);
//                        // Export the modulus
//                        var modulus = rsaParameters.Modulus.ToBase64Simplified();
//                        // Export the exponent
//                        var exponent = rsaParameters.Exponent.ToBase64Simplified();

//                        result.Add(JwtConstants.JsonWebKeyParameterNames.RsaKey.ModulusName, modulus);
//                        result.Add(JwtConstants.JsonWebKeyParameterNames.RsaKey.ExponentName, exponent);
//                    }
//                }
//            }
//        }
//    }
//}
