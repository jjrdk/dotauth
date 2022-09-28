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

namespace SimpleAuth.Tests.Api.Registration;

using Helpers;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Shared;
using Shared.Models;
using Shared.Repositories;
using SimpleAuth;
using SimpleAuth.Shared.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SimpleAuth.Extensions;
using SimpleAuth.Tests.Validators;
using Xunit;

public sealed class DefaultClientRepositoryFixture
{
    private readonly IClientRepository _clientRepositoryFake;

    public DefaultClientRepositoryFixture()
    {
        _clientRepositoryFake = new InMemoryClientRepository(
            new TestHttpClientFactory(),
            new InMemoryScopeRepository(new[] { new Scope { Name = "scope" } }),
            new Mock<ILogger<InMemoryClientRepository>>().Object,
            Array.Empty<Client>());
    }

    [Fact]
    public async Task When_Client_Does_Not_Exist_Then_ReturnsEmptyResult()
    {
        const string clientId = "client_id";

        var result = await _clientRepositoryFake.Search(
                new SearchClientsRequest { ClientIds = new[] { clientId } },
                CancellationToken.None)
            .ConfigureAwait(false);
        Assert.Empty(result.Content);
    }

    [Fact]
    public async Task When_Getting_Client_Then_Information_Are_Returned()
    {
        var client = new Client
        {
            ClientId = "test",
            JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
            AllowedScopes = new[] { "scope" },
            RedirectionUrls = new[] { new Uri("https://localhost"), },
            RequestUris = new[] { new Uri("https://localhost"), }
        };
        _ = await _clientRepositoryFake.Insert(client, CancellationToken.None).ConfigureAwait(false);

        var result = await _clientRepositoryFake.Search(
                new SearchClientsRequest { ClientIds = new[] { client.ClientId } },
                CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(client.ClientId, result.Content.First().ClientId);
    }

    [Fact]
    public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
    {
        await Assert
            .ThrowsAsync<ArgumentNullException>(() => _clientRepositoryFake.Insert(null, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task When_Passing_Registration_Parameter_With_Specific_Values_Then_ReturnsTrue()
    {
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
            ClientName = clientName,
            ResponseTypes = new[] { ResponseTypeNames.Token },
            GrantTypes = new[] { GrantTypes.Implicit },
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "test" } },
            AllowedScopes = new[] { "scope" },
            ApplicationType = ApplicationTypes.Native,
            ClientUri = clientUri,
            PolicyUri = policyUri,
            TosUri = tosUri,
            //JwksUri = jwksUri,
            JsonWebKeys = new List<JsonWebKey> { new() { Kid = kid } }.ToJwks(),
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
            TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretBasic,
            TokenEndPointAuthSigningAlg = SecurityAlgorithms.RsaSha256,
            //DefaultMaxAge = defaultMaxAge,
            DefaultAcrValues = defaultAcrValues,
            RequireAuthTime = requireAuthTime,
            InitiateLoginUri = initiateLoginUri,
            RequestUris = new[] { requestUri }
        };

        var result = await _clientRepositoryFake.Insert(client, CancellationToken.None).ConfigureAwait(false);
        Assert.True(result);
    }
}