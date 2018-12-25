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

namespace SimpleIdentityServer.Core.UnitTests.Api.Registration
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SimpleAuth;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Parameters;
    using SimpleAuth.Shared.Repositories;
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
        public async Task When_Client_Doesnt_Exist_Then_ReturnsEmptyResult()
        {
            const string clientId = "client_id";
            InitializeFakeObjects();

            var result = await _clientRepositoryFake.Search(new SearchClientParameter { ClientIds = new[] { clientId } }).ConfigureAwait(false);
            Assert.Empty(result.Content);
        }

        [Fact]
        public async Task When_Getting_Client_Then_Information_Are_Returned()
        {
            const string clientId = "clientId";
            var client = new Client
            {
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

            await Assert.ThrowsAsync<ArgumentNullException>(() => _clientRepositoryFake.Insert(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Registration_Parameter_With_Specific_Values_Then_ReturnsTrue()
        {
            InitializeFakeObjects();
            const string clientName = "client_name";
            var clientUri = new Uri("https://client_uri", UriKind.Absolute);
            var policyUri = new Uri("https://policy_uri", UriKind.Absolute);
            var tosUri = new Uri("https://tos_uri", UriKind.Absolute);
            var jwksUri = new Uri("https://jwks_uri", UriKind.Absolute);
            const string kid = "kid";
            //var sectorIdentifierUri = new Uri("https://sector_identifier_uri", UriKind.Absolute);
            const double defaultMaxAge = 3;
            const string defaultAcrValues = "default_acr_values";
            const bool requireAuthTime = false;
            var initiateLoginUri = new Uri("https://initiate_login_uri", UriKind.Absolute);
            var requestUri = new Uri("https://request_uri", UriKind.Absolute);

            var client = new Client
            {
                ClientId = "testclient",
                ClientName = clientName,
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                },
                GrantTypes = new List<GrantType>
                {
                    GrantType.@implicit
                },
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret{ Type = ClientSecretTypes.SharedSecret, Value = "test"}
                },
                AllowedScopes = new[] { new Scope { Name = "scope" } },
                ApplicationType = ApplicationTypes.native,
                ClientUri = clientUri,
                PolicyUri = policyUri,
                TosUri = tosUri,
                JwksUri = jwksUri,
                JsonWebKeys = new List<JsonWebKey>
                {
                    new JsonWebKey
                    {
                        Kid = kid
                    }
                },
                RedirectionUrls = new[] { new Uri("https://localhost"), },
                //SectorIdentifierUri = sectorIdentifierUri,
                IdTokenSignedResponseAlg = JwtConstants.JwsAlgNames.RS256,
                IdTokenEncryptedResponseAlg = JwtConstants.JweAlgNames.RSA1_5,
                IdTokenEncryptedResponseEnc = JwtConstants.JweEncNames.A128CBC_HS256,
                UserInfoSignedResponseAlg = JwtConstants.JwsAlgNames.RS256,
                UserInfoEncryptedResponseAlg = JwtConstants.JweAlgNames.RSA1_5,
                UserInfoEncryptedResponseEnc = JwtConstants.JweEncNames.A128CBC_HS256,
                RequestObjectSigningAlg = JwtConstants.JwsAlgNames.RS256,
                RequestObjectEncryptionAlg = JwtConstants.JweAlgNames.RSA1_5,
                RequestObjectEncryptionEnc = JwtConstants.JweEncNames.A128CBC_HS256,
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
                TokenEndPointAuthSigningAlg = JwtConstants.JwsAlgNames.RS256,
                DefaultMaxAge = defaultMaxAge,
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
