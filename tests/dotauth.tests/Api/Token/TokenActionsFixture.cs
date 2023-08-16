﻿namespace DotAuth.Tests.Api.Token;

using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DotAuth;
using DotAuth.Api.Token;
using DotAuth.Controllers;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Repositories;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

public sealed class TokenActionsFixture
{
    private const string ClientId = "valid_client_id";
    private const string Clientsecret = "secret";
    private readonly TokenActions _tokenActions;

    public TokenActionsFixture()
    {
        var eventPublisher = Substitute.For<IEventPublisher>();
        const string scope = "valid_scope";
        var mock = Substitute.For<IClientStore>();
        mock.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                new Client
                {
                    JsonWebKeys =
                        "supersecretlongkey".CreateJwk(
                                JsonWebKeyUseNames.Sig,
                                KeyOperations.Sign,
                                KeyOperations.Verify)
                            .ToSet(),
                    IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
                    ClientId = ClientId,
                    Secrets =
                        new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = Clientsecret } },
                    AllowedScopes = new[] { scope },
                    ResponseTypes = new[] { ResponseTypeNames.Token },
                    GrantTypes = new[] { GrantTypes.ClientCredentials }
                });

        _tokenActions = new TokenActions(
            new RuntimeSettings(string.Empty),
            Substitute.For<IAuthorizationCodeStore>(),
            mock,
            Substitute.For<IScopeRepository>(),
            new InMemoryJwksRepository(),
            new InMemoryResourceOwnerRepository(string.Empty),
            Array.Empty<IAuthenticateResourceOwnerService>(),
            eventPublisher,
            Substitute.For<ITokenStore>(),
            Substitute.For<IDeviceAuthorizationStore>(),
            Substitute.For<ILogger<TokenController>>());
    }

    [Fact]
    public async Task When_Passing_No_Request_To_ResourceOwner_Grant_Type_Then_Error_Is_Returned()
    {
        await Assert.ThrowsAsync<NullReferenceException>(
                () => _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(
                    null,
                    null,
                    null,
                    null,
                    CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task When_Passing_No_Request_To_AuthorizationCode_Grant_Type_Then_Exception_Is_Thrown()
    {
        await Assert.ThrowsAsync<NullReferenceException>(
                () => _tokenActions.GetTokenByAuthorizationCodeGrantType(
                    null,
                    null,
                    null,
                    null,
                    CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task When_Passing_No_Request_To_Refresh_Token_Grant_Type_Then_Exception_Is_Thrown()
    {
        await Assert.ThrowsAsync<NullReferenceException>(
                () => _tokenActions.GetTokenByRefreshTokenGrantType(null, null, null, null, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task When_Passing_Null_Parameter_To_ClientCredentials_GrantType_Then_Exception_Is_Thrown()
    {
        await Assert.ThrowsAsync<NullReferenceException>(
                () => _tokenActions.GetTokenByClientCredentialsGrantType(
                    null,
                    null,
                    null,
                    null,
                    CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task When_Getting_Token_Via_ClientCredentials_GrantType_Then_GrantedToken_Is_Returned()
    {
        const string scope = "valid_scope";
        const string clientId = "valid_client_id";
        var parameter = new ClientCredentialsGrantTypeParameter { Scope = scope };

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{Clientsecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Result>(await _tokenActions
            .GetTokenByClientCredentialsGrantType(
                parameter,
                authenticationHeader,
                null,
                null,
                CancellationToken.None)
            .ConfigureAwait(false));

        Assert.Equal(clientId, result.Item.ClientId);
    }

    [Fact]
    public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
    {
        await Assert
            .ThrowsAsync<NullReferenceException>(
                () => _tokenActions.RevokeToken(null, null, null, null, CancellationToken.None))
            .ConfigureAwait(false);
    }
}
