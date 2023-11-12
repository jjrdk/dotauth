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

namespace DotAuth.Tests.Api.Token;

using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using DotAuth.Api.Token.Actions;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Repositories;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.OAuth;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;
using DotAuth.Tests.Helpers;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;
using Xunit.Abstractions;

public sealed class GetTokenByResourceOwnerCredentialsGrantTypeActionFixture
{
    private readonly ITestOutputHelper _outputHelper;
    private IEventPublisher _eventPublisher = null!;
    private IClientStore _clientStore = null!;
    private ITokenStore _tokenStoreStub = null!;
    private GetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantTypeAction = null!;
    private readonly IScopeRepository _scopeRepository;

    public GetTokenByResourceOwnerCredentialsGrantTypeActionFixture(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _scopeRepository = Substitute.For<IScopeRepository>();
    }

    [Fact]
    public async Task When_Client_Cannot_Be_Authenticated_Then_Error_Is_Returned()
    {
        InitializeFakeObjects();
        const string clientAssertion = "clientAssertion";
        const string clientAssertionType = "clientAssertionType";
        const string clientId = "clientId";
        const string clientSecret = "clientSecret";
        var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
        {
            ClientAssertion = clientAssertion,
            ClientAssertionType = clientAssertionType,
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(
            await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeader,
                    null,
                    "",
                    CancellationToken.None)
                );

        Assert.Equal(ErrorCodes.InvalidClient, result.Details.Title);
        Assert.Equal(string.Format(SharedStrings.TheClientDoesntExist), result.Details.Detail);
    }

    [Fact]
    public async Task When_Client_GrantType_Is_Not_Valid_Then_Error_Is_Returned()
    {
        InitializeFakeObjects();
        const string clientAssertion = "clientAssertion";
        const string clientAssertionType = "clientAssertionType";
        const string clientId = "clientId";
        const string clientSecret = "clientSecret";
        var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
        {
            ClientAssertion = clientAssertion,
            ClientAssertionType = clientAssertionType,
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        var client = new Client
        {
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.AuthorizationCode }
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(
            await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeader,
                    null,
                    "",
                    CancellationToken.None)
                );

        Assert.Equal(ErrorCodes.InvalidGrant, result.Details.Title);
        Assert.Equal(
            string.Format(Strings.TheClientDoesntSupportTheGrantType, clientId, GrantTypes.Password),
            result.Details.Detail);
    }

    [Fact]
    public async Task When_Client_ResponseTypes_Are_Not_Valid_Then_Error_Is_Returned()
    {
        InitializeFakeObjects();
        const string clientAssertion = "clientAssertion";
        const string clientAssertionType = "clientAssertionType";
        const string clientId = "clientId";
        const string clientSecret = "clientSecret";
        var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
        {
            ClientAssertion = clientAssertion,
            ClientAssertionType = clientAssertionType,
            ClientId = clientId,
            ClientSecret = clientSecret
        };
        var client = new Client
        {
            ResponseTypes = Array.Empty<string>(),
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.Password }
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(
            await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeader,
                    null,
                    "",
                    CancellationToken.None)
                );

        Assert.Equal(ErrorCodes.InvalidResponse, result.Details.Title);
        Assert.Equal(
            string.Format(Strings.TheClientDoesntSupportTheResponseType, clientId, "token id_token"),
            result.Details.Detail);
    }

    [Fact]
    public async Task When_The_Resource_Owner_Is_Not_Valid_Then_Error_Is_Returned()
    {
        const string clientAssertion = "clientAssertion";
        const string clientAssertionType = "clientAssertionType";
        const string clientId = "clientId";
        const string clientSecret = "clientSecret";
        var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
        {
            ClientAssertion = clientAssertion,
            ClientAssertionType = clientAssertionType,
            ClientId = clientId,
            ClientSecret = clientSecret
        };
        var client = new Client
        {
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.Password },
            ResponseTypes = new[] { ResponseTypeNames.IdToken, ResponseTypeNames.Token }
        };

        var authenticateService = Substitute.For<IAuthenticateResourceOwnerService>();
        authenticateService.Amr.Returns("pwd");
        authenticateService.AuthenticateResourceOwner(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .ReturnsNull();
        InitializeFakeObjects(authenticateService);
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);
        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(
            await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeader,
                    null,
                    "",
                    CancellationToken.None)
                );
        Assert.Equal(ErrorCodes.InvalidCredentials, result.Details.Title);
        Assert.Equal(Strings.ResourceOwnerCredentialsAreNotValid, result.Details.Detail);
    }

    [Fact]
    public async Task When_Passing_A_Not_Allowed_Scopes_Then_Error_Is_Returned()
    {
        const string clientAssertion = "clientAssertion";
        const string clientAssertionType = "clientAssertionType";
        const string clientId = "clientId";
        const string clientSecret = "clientSecret";
        const string invalidScope = "invalidScope";
        var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
        {
            ClientAssertion = clientAssertion,
            ClientAssertionType = clientAssertionType,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scope = invalidScope
        };
        var client = new Client
        {
            ClientId = "id",
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.Password },
            ResponseTypes = new[] { ResponseTypeNames.IdToken, ResponseTypeNames.Token }
        };

        var resourceOwner = new ResourceOwner();
        var authenticateService = Substitute.For<IAuthenticateResourceOwnerService>();
        authenticateService.AuthenticateResourceOwner(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(resourceOwner);
        authenticateService.Amr.Returns("pwd");
        InitializeFakeObjects(authenticateService);
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(
            await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                    resourceOwnerGrantTypeParameter,
                    authenticationHeader,
                    null,
                    "",
                    CancellationToken.None)
                );

        Assert.Equal(ErrorCodes.InvalidScope, result.Details.Title);
    }

    [Fact]
    public async Task When_Requesting_An_AccessToken_For_An_Authenticated_User_Then_AccessToken_Is_Granted()
    {
        const string clientAssertion = "clientAssertion";
        const string clientAssertionType = "clientAssertionType";
        const string clientId = "clientId";
        const string clientSecret = "clientSecret";
        const string invalidScope = "invalidScope";
        //const string accessToken = "accessToken";
        var resourceOwnerGrantTypeParameter = new ResourceOwnerGrantTypeParameter
        {
            ClientAssertion = clientAssertion,
            ClientAssertionType = clientAssertionType,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scope = invalidScope
        };
        var client = new Client
        {
            AllowedScopes = new[] { invalidScope },
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            JsonWebKeys =
                TestKeys.SecretKey.CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify)
                    .ToSet(),
            IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256,
            GrantTypes = new[] { GrantTypes.Password },
            ResponseTypes = new[] { ResponseTypeNames.IdToken, ResponseTypeNames.Token }
        };
        var resourceOwner = new ResourceOwner { Subject = "tester" };
        var authenticateService = Substitute.For<IAuthenticateResourceOwnerService>();
        authenticateService.AuthenticateResourceOwner(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(resourceOwner);
        authenticateService.Amr.Returns("pwd");
        InitializeFakeObjects(authenticateService);
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);
        _scopeRepository.SearchByNames(Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns(new[] { new Scope { Name = invalidScope } });

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        await _getTokenByResourceOwnerCredentialsGrantTypeAction.Execute(
                resourceOwnerGrantTypeParameter,
                authenticationHeader,
                null,
                "issuer",
                CancellationToken.None)
            ;

        await _tokenStoreStub.Received().AddToken(Arg.Any<GrantedToken>(), Arg.Any<CancellationToken>());
        await _eventPublisher.Received().Publish(Arg.Any<TokenGranted>());
    }

    private void InitializeFakeObjects(params IAuthenticateResourceOwnerService[] services)
    {
        _eventPublisher = Substitute.For<IEventPublisher>();
        _eventPublisher.Publish(Arg.Any<TokenGranted>()).Returns(Task.CompletedTask);
        _clientStore = Substitute.For<IClientStore>();
        _tokenStoreStub = Substitute.For<ITokenStore>();

        _getTokenByResourceOwnerCredentialsGrantTypeAction = new GetTokenByResourceOwnerCredentialsGrantTypeAction(
            _clientStore,
            _scopeRepository,
            _tokenStoreStub,
            new InMemoryJwksRepository(),
            services,
            _eventPublisher,
            new TestOutputLogger("test", _outputHelper));
    }
}
