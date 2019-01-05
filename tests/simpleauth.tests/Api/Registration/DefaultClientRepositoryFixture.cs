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

namespace SimpleAuth.Tests.Api.Registration
{
    using Helpers;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using Repositories;
    using Shared;
    using Shared.Models;
    using Shared.Parameters;
    using Shared.Repositories;
    using SimpleAuth;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    public sealed class DefaultClientRepositoryFixture : IDisposable
    {
        private IClientRepository _clientRepositoryFake;
        private readonly HttpClient _httpClient;

        public DefaultClientRepositoryFixture()
        {
            _httpClient = new HttpClient();
        }

        [Fact]
        public async Task When_Client_Does_Not_Exist_Then_ReturnsEmptyResult()
        {
            const string clientId = "client_id";
            InitializeFakeObjects();

            var result = await _clientRepositoryFake.Search(new SearchClientParameter { ClientIds = new[] { clientId } })
                .ConfigureAwait(false);
            Assert.Empty(result.Content);
        }

        [Fact]
        public async Task When_Getting_Client_Then_Information_Are_Returned()
        {
            const string clientId = "clientId";
            var client = new Client
            {
                JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
                ClientId = clientId,
                AllowedScopes = new[] { new Scope { Name = "scope" } },
                RedirectionUrls = new[] { new Uri("https://localhost"), },
                RequestUris = new[] { new Uri("https://localhost"), }
            };
            InitializeFakeObjects();
            await _clientRepositoryFake.Insert(client).ConfigureAwait(false);

            var result = await _clientRepositoryFake.Search(
                    new SearchClientParameter { ClientIds = new[] { clientId } })
                .ConfigureAwait(false);

            Assert.True(result.Content.First().ClientId == clientId);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientRepositoryFake.Insert(null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Registration_Parameter_With_Specific_Values_Then_ReturnsTrue()
        {
            InitializeFakeObjects();
            const string clientName = "client_name";
            var clientUri = new Uri("https://client_uri", UriKind.Absolute);
            var policyUri = new Uri("https://policy_uri", UriKind.Absolute);
            var tosUri = new Uri("https://tos_uri", UriKind.Absolute);
            const string kid = "kid";
            //var sectorIdentifierUri = new Uri("https://sector_identifier_uri", UriKind.Absolute);
            const string defaultAcrValues = "default_acr_values";
            const bool requireAuthTime = false;
            var initiateLoginUri = new Uri("https://initiate_login_uri", UriKind.Absolute);
            var requestUri = new Uri("https://request_uri", UriKind.Absolute);

            var client = new Client
            {
                ClientId = "testclient",
                ClientName = clientName,
                ResponseTypes = new[]
                {
                    ResponseTypeNames.Token
                },
                GrantTypes = new List<GrantType>
                {
                    GrantType.@implicit
                },
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret {Type = ClientSecretTypes.SharedSecret, Value = "test"}
                },
                AllowedScopes = new[] { new Scope { Name = "scope" } },
                ApplicationType = ApplicationTypes.native,
                ClientUri = clientUri,
                PolicyUri = policyUri,
                TosUri = tosUri,
                //JwksUri = jwksUri,
                JsonWebKeys = new List<JsonWebKey>
                {
                    new JsonWebKey
                    {
                        Kid = kid
                    }
                }.ToJwks(),
                RedirectionUrls = new[] { new Uri("https://localhost"), },
                //SectorIdentifierUri = sectorIdentifierUri,
                IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                IdTokenEncryptedResponseAlg = SecurityAlgorithms.RsaPKCS1,
                IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                UserInfoSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                UserInfoEncryptedResponseAlg = SecurityAlgorithms.RsaPKCS1,
                UserInfoEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                RequestObjectSigningAlg = SecurityAlgorithms.RsaSha256,
                RequestObjectEncryptionAlg = SecurityAlgorithms.RsaPKCS1,
                RequestObjectEncryptionEnc = SecurityAlgorithms.Aes128CbcHmacSha256,
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                TokenEndPointAuthSigningAlg = SecurityAlgorithms.RsaSha256,
                //DefaultMaxAge = defaultMaxAge,
                DefaultAcrValues = defaultAcrValues,
                RequireAuthTime = requireAuthTime,
                InitiateLoginUri = initiateLoginUri,
                RequestUris = new List<Uri>
                {
                    requestUri
                }
            };

            var jsonClient = JsonConvert.SerializeObject(client);
            var result = await _clientRepositoryFake.Insert(client).ConfigureAwait(false);
            var jsonResult = JsonConvert.SerializeObject(result);

            Assert.Equal(jsonClient, jsonResult);
        }

        private void InitializeFakeObjects()
        {
            _clientRepositoryFake =
                new DefaultClientRepository(
                    new Client[0],
                    _httpClient,
                    new DefaultScopeRepository(new[] { new Scope { Name = "scope" } }));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
