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

namespace DotAuth.Tests.Api.Registration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using DotAuth.Tests.Helpers;
using DotAuth.Tests.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

public sealed class DefaultClientRepositoryFixture
{
    private readonly IClientRepository _clientRepositoryFake;

    public DefaultClientRepositoryFixture()
    {
        _clientRepositoryFake = new InMemoryClientRepository(
            new TestHttpClientFactory(),
            new InMemoryScopeRepository([new Scope { Name = "scope" }]),
            Substitute.For<ILogger<InMemoryClientRepository>>(),
            Array.Empty<Client>());
    }

    [Fact]
    public async Task When_Client_Does_Not_Exist_Then_ReturnsEmptyResult()
    {
        const string clientId = "client_id";

        var result = await _clientRepositoryFake.Search(
                new SearchClientsRequest { ClientIds = [clientId] },
                CancellationToken.None)
            ;
        Assert.Empty(result.Content);
    }

    [Fact]
    public async Task When_Getting_Client_Then_Information_Are_Returned()
    {
        var client = new Client
        {
            ClientId = "test",
            JsonWebKeys = TestKeys.SecretKey.CreateSignatureJwk().ToSet(),
            AllowedScopes = ["scope"],
            RedirectionUrls = [new Uri("https://localhost")]
        };
        _ = await _clientRepositoryFake.Insert(client, CancellationToken.None);

        var result = await _clientRepositoryFake.Search(
                new SearchClientsRequest { ClientIds = [client.ClientId] },
                CancellationToken.None)
            ;

        Assert.Equal(client.ClientId, result.Content.First().ClientId);
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

        var client = new Client
        {
            ClientName = clientName,
            ResponseTypes = [ResponseTypeNames.Token],
            GrantTypes = [GrantTypes.Implicit],
            Secrets = [new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "test" }],
            AllowedScopes = ["scope"],
            ApplicationType = ApplicationTypes.Native,
            ClientUri = clientUri,
            PolicyUri = policyUri,
            TosUri = tosUri,
            //JwksUri = jwksUri,
            JsonWebKeys = new List<JsonWebKey> { new() { Kid = kid } }.ToJwks(),
            RedirectionUrls = [new Uri("https://localhost")],
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
            InitiateLoginUri = initiateLoginUri
        };

        var result = await _clientRepositoryFake.Insert(client, CancellationToken.None);
        Assert.True(result);
    }
}
