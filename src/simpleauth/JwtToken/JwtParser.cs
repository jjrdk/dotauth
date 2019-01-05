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

//namespace SimpleAuth.JwtToken
//{
//    using Errors;
//    using Microsoft.IdentityModel.Tokens;
//    using Shared.Models;
//    using Shared.Repositories;
//    using Shared.Requests;
//    using System;
//    using System.IdentityModel.Tokens.Jwt;
//    using System.Linq;
//    using System.Net.Http;
//    using System.Threading.Tasks;
//    using JwtConstants = SimpleAuth.JwtConstants;

//    public class JwtParser : IJwtParser
//    {
//        private const int JweSegmentCount = 5;
//        private const int JwsSegmentCount = 3;
//        private readonly HttpClient _httpClientFactory;
//        private readonly IClientStore _clientRepository;
//        private readonly IJsonWebKeyRepository _jsonWebKeyRepository;
//        private readonly JwtSecurityTokenHandler _handler = new JwtSecurityTokenHandler();

//        public JwtParser(
//            HttpClient httpClientFactory,
//            IClientStore clientRepository,
//            IJsonWebKeyRepository jsonWebKeyRepository)
//        {
//            _httpClientFactory = httpClientFactory;
//            _clientRepository = clientRepository;
//            _jsonWebKeyRepository = jsonWebKeyRepository;
//        }

//        public bool IsJweToken(string jwe)
//        {
//            return jwe.Split('.').Length == JweSegmentCount;
//        }

//        public bool IsJwsToken(string jws)
//        {
//            return jws.Split('.').Length == JwsSegmentCount;
//        }

//        public Task<JwtSecurityToken> DecryptAsync(string jwe)
//        {
//            if (string.IsNullOrWhiteSpace(jwe))
//            {
//                throw new ArgumentNullException(nameof(jwe));
//            }

//            var token = _handler.ReadJwtToken(jwe);
//            return Task.FromResult(token);
//            //var protectedHeader = _jweParser.GetHeader(jwe);
//            //if (protectedHeader == null)
//            //{
//            //    return string.Empty;
//            //}

//            //var jsonWebKey = await _jsonWebKeyRepository.GetByKidAsync(protectedHeader.Kid).ConfigureAwait(false);
//            //if (jsonWebKey == null)
//            //{
//            //    return string.Empty;
//            //}

//            //return _jweParser.Parse(jwe, jsonWebKey);
//        }

//        public async Task<string> DecryptAsync(string jwe, string clientId)
//        {
//            var jsonWebKey = await GetJsonWebKeyToDecrypt(jwe, clientId).ConfigureAwait(false);
//            if (jsonWebKey == null)
//            {
//                return string.Empty;
//            }

//            return _jweParser.Parse(jwe, jsonWebKey);
//        }

//        public async Task<string> DecryptWithPasswordAsync(string jwe, string clientId, string password)
//        {
//            var jsonWebKey = await GetJsonWebKeyToDecrypt(jwe, clientId).ConfigureAwait(false);
//            if (jsonWebKey == null)
//            {
//                return string.Empty;
//            }

//            return _jweParser.ParseByUsingSymmetricPassword(jwe, jsonWebKey, password);
//        }

//        public async Task<JwtPayload> UnSignAsync(string jws)
//        {
//            if (string.IsNullOrWhiteSpace(jws))
//            {
//                throw new ArgumentNullException(nameof(jws));
//            }

//            var protectedHeader = _jwsParser.GetHeader(jws);
//            if (protectedHeader == null)
//            {
//                return null;
//            }

//            var jsonWebKey = await _jsonWebKeyRepository.GetByKidAsync(protectedHeader.Kid).ConfigureAwait(false);
//            return UnSignWithJsonWebKey(jsonWebKey, protectedHeader, jws);
//        }

//        public async Task<JwtPayload> UnSignAsync(string jws, string clientId)
//        {
//            if (string.IsNullOrWhiteSpace(jws))
//            {
//                throw new ArgumentNullException(nameof(jws));
//            }

//            if (string.IsNullOrWhiteSpace(clientId))
//            {
//                throw new ArgumentNullException(nameof(clientId));
//            }

//            var client = await _clientRepository.GetById(clientId).ConfigureAwait(false);
//            if (client == null)
//            {
//                throw new InvalidOperationException(string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
//            }

//            var protectedHeader = _jwsParser.GetHeader(jws);
//            if (protectedHeader == null)
//            {
//                return null;
//            }

//            var jsonWebKey = await GetJsonWebKeyFromClient(client, protectedHeader.Kid).ConfigureAwait(false);
//            return UnSignWithJsonWebKey(jsonWebKey, protectedHeader, jws);
//        }

//        private JwtSecurityToken UnSignWithJsonWebKey(JsonWebKey jsonWebKey, JwsProtectedHeader jwsProtectedHeader, string jws)
//        {
//            if (jsonWebKey == null
//                && jwsProtectedHeader.Alg != SecurityAlgorithms.None)
//            {
//                return null;
//            }

//            if (jwsProtectedHeader.Alg == SecurityAlgorithms.None)
//            {
//                return _jwsParser.GetPayload(jws);
//            }

//            return _jwsParser.ValidateSignature(jws, jsonWebKey);
//        }

//        private async Task<JsonWebKey> GetJsonWebKeyToDecrypt(string jwe, string clientId)
//        {
//            if (string.IsNullOrWhiteSpace(jwe))
//            {
//                throw new ArgumentNullException(nameof(jwe));
//            }

//            if (string.IsNullOrWhiteSpace(clientId))
//            {
//                throw new ArgumentNullException(nameof(clientId));
//            }

//            var client = await _clientRepository.GetById(clientId).ConfigureAwait(false);
//            if (client == null)
//            {
//                throw new InvalidOperationException(string.Format(ErrorDescriptions.ClientIsNotValid, clientId));
//            }

//            var protectedHeader = _jweParser.GetHeader(jwe);
//            if (protectedHeader == null)
//            {
//                return null;
//            }

//            var jsonWebKey = await GetJsonWebKeyFromClient(client, protectedHeader.Kid).ConfigureAwait(false);
//            return jsonWebKey;
//        }

//        private async Task<JsonWebKey> GetJsonWebKeyFromClient(Client client, string kid)
//        {
//            JsonWebKey result = null;
//            // Fetch the json web key from the jwks_uri
//            if (client.JwksUri != null)
//            {
//                try
//                {
//                    var request = await _httpClientFactory.GetAsync(client.JwksUri).ConfigureAwait(false);
//                    request.EnsureSuccessStatusCode();
//                    var json = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
//                    var jsonWebKeySet = json.DeserializeWithJavascript<JsonWebKeySet>();
//                    //var jsonWebKeys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
//                    return jsonWebKeySet.Keys.FirstOrDefault(j => j.Kid == kid);
//                }
//                catch (Exception)
//                {
//                    return null;
//                }
//            }

//            // Fetch the json web key from the jwks
//            if (client.JsonWebKeys != null &&
//                client.JsonWebKeys.Any())
//            {
//                result = client.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
//            }

//            return result;
//        }
//    }
//}
