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
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth;
using DotAuth.Api.Token.Actions;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.OAuth;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

public sealed class GetTokenByAuthorizationCodeGrantTypeActionFixture
{
    private IEventPublisher _eventPublisher = null!;
    private IAuthorizationCodeStore _authorizationCodeStoreFake = null!;
    private RuntimeSettings _dotAuthOptions = null!;
    private ITokenStore _tokenStoreFake = null!;
    private IClientStore _clientStore = null!;
    private GetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction = null!;
    private InMemoryJwksRepository _inMemoryJwksRepository = null!;

    public GetTokenByAuthorizationCodeGrantTypeActionFixture()
    {
        IdentityModelEventSource.ShowPII = true;
    }

    [Fact]
    public async Task When_Client_Cannot_Be_Authenticated_Then_Error_Is_Returned()
    {
        InitializeFakeObjects();
        var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
        {
            Code = "abc",
            ClientAssertion = "clientAssertion",
            ClientAssertionType = "clientAssertionType",
            ClientId = "clientId",
            ClientSecret = "clientSecret"
        };

        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsNull();

        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByAuthorizationCodeGrantTypeAction
            .Execute(
                authorizationCodeGrantTypeParameter,
                null,
                null,
                "",
                CancellationToken.None)
        );
        Assert.Equal(ErrorCodes.InvalidClient, result.Details.Title);
    }

    [Fact]
    public async Task When_Client_Does_Not_Support_Grant_Type_Code_Then_Error_Is_Returned()
    {
        InitializeFakeObjects();
        var clientId = "clientId";
        var clientSecret = "clientSecret";
        var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
        {
            Code = "abc",
            ClientAssertion = "clientAssertion",
            ClientAssertionType = "clientAssertionType",
            ClientId = clientId,
            ClientSecret = clientSecret
        };
        var client = new Client
        {
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } }
        };
        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByAuthorizationCodeGrantTypeAction
            .Execute(
                authorizationCodeGrantTypeParameter,
                authenticationHeader,
                null,
                "",
                CancellationToken.None)
        );
        Assert.Equal(ErrorCodes.InvalidGrant, result.Details.Title);
        Assert.Equal(
            string.Format(
                Strings.TheClientDoesntSupportTheGrantType,
                clientId,
                GrantTypes.AuthorizationCode),
            result.Details.Detail);
    }

    [Fact]
    public async Task When_Client_Does_Not_Support_ResponseType_Code_Then_Error_Is_Returned()
    {
        InitializeFakeObjects();
        var clientId = "clientId";
        var clientSecret = "clientSecret";
        var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
        {
            Code = "abc",
            ClientAssertion = "clientAssertion",
            ClientAssertionType = "clientAssertionType",
            ClientId = clientId,
            ClientSecret = clientSecret
        };
        var client = new Client
        {
            ResponseTypes = Array.Empty<string>(),
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.AuthorizationCode }
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        var authenticationValue = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByAuthorizationCodeGrantTypeAction
            .Execute(
                authorizationCodeGrantTypeParameter,
                authenticationValue,
                null,
                "",
                CancellationToken.None)
        );
        Assert.Equal(ErrorCodes.InvalidResponse, result.Details.Title);
        Assert.Equal(
            string.Format(
                Strings.TheClientDoesntSupportTheResponseType,
                clientId,
                ResponseTypeNames.Code),
            result.Details.Detail);
    }

    [Fact]
    public async Task When_Authorization_Code_Is_Not_Valid_Then_Error_Is_Returned()
    {
        InitializeFakeObjects();
        var clientSecret = "clientSecret";
        var clientId = "id";
        var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
        {
            ClientAssertion = "clientAssertion",
            ClientAssertionType = "clientAssertionType",
            ClientId = clientId,
            ClientSecret = clientSecret
        };
        var client = new Client
        {
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.AuthorizationCode },
            ResponseTypes = new[] { ResponseTypeNames.Code }
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        _authorizationCodeStoreFake.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(default(AuthorizationCode)));
        var authorizationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByAuthorizationCodeGrantTypeAction
            .Execute(
                authorizationCodeGrantTypeParameter,
                authorizationHeader,
                null,
                "",
                CancellationToken.None)
        );
        Assert.Equal(ErrorCodes.InvalidGrant, result.Details.Title);
        Assert.Equal(Strings.TheAuthorizationCodeIsNotCorrect, result.Details.Detail);
    }

    [Fact]
    public async Task When_Pkce_Validation_Failed_Then_Error_Is_Returned()
    {
        InitializeFakeObjects();
        var clientId = "clientId";
        var clientSecret = "clientSecret";
        var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
        {
            Code = "xyz",
            CodeVerifier = "abc",
            ClientAssertion = "clientAssertion",
            ClientAssertionType = "clientAssertionType",
            ClientId = clientId,
            ClientSecret = clientSecret
        };
        var authorizationCode = new AuthorizationCode { ClientId = clientId };
        var client = new Client
        {
            RequirePkce = true,
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.AuthorizationCode },
            ResponseTypes = new[] { ResponseTypeNames.Code }
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        _authorizationCodeStoreFake.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(authorizationCode);

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByAuthorizationCodeGrantTypeAction
            .Execute(
                authorizationCodeGrantTypeParameter,
                authenticationHeader,
                null,
                "",
                CancellationToken.None)
        );
        Assert.Equal(ErrorCodes.InvalidRequest, result.Details.Title);
        Assert.Equal(Strings.TheCodeVerifierIsNotCorrect, result.Details.Detail);
    }

    [Fact]
    public async Task When_Granted_Client_Is_Not_The_Same_Then_Error_Is_Returned()
    {
        InitializeFakeObjects();
        var clientSecret = "clientSecret";
        var clientId = "notCorrectClientId";
        var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
        {
            Code = "abc",
            ClientAssertion = "clientAssertion",
            ClientAssertionType = "clientAssertionType",
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        var client = new Client
        {
            RequirePkce = false,
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.AuthorizationCode },
            ResponseTypes = new[] { ResponseTypeNames.Code }
        };
        var authorizationCode = new AuthorizationCode { ClientId = "clientId" };

        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);
        _authorizationCodeStoreFake.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<AuthorizationCode?>(authorizationCode));

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByAuthorizationCodeGrantTypeAction
            .Execute(
                authorizationCodeGrantTypeParameter,
                authenticationHeader,
                null,
                "",
                CancellationToken.None)
        );
        Assert.Equal(ErrorCodes.InvalidRequest, result.Details.Title);
        Assert.Equal(
            string.Format(
                Strings.TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId,
                "clientId"),
            result.Details.Detail);
    }

    [Fact]
    public async Task When_Redirect_Uri_Is_Not_The_Same_Then_Error_Is_Returned()
    {
        InitializeFakeObjects();
        var clientId = "clientId";
        var clientSecret = "clientSecret";
        var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
        {
            Code = "abc",
            ClientAssertion = "clientAssertion",
            ClientAssertionType = "clientAssertionType",
            ClientId = clientId,
            ClientSecret = clientSecret,
            RedirectUri = new Uri("https://notCorrectRedirectUri")
        };

        var client = new Client
        {
            RequirePkce = false,
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.AuthorizationCode },
            ResponseTypes = new[] { ResponseTypeNames.Code }
        };
        var authorizationCode = new AuthorizationCode
        {
            ClientId = clientId,
            RedirectUri = new Uri("https://redirectUri")
        };

        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);
        _authorizationCodeStoreFake.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(authorizationCode);
        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByAuthorizationCodeGrantTypeAction
            .Execute(
                authorizationCodeGrantTypeParameter,
                authenticationHeader,
                null,
                "",
                CancellationToken.None)
        );
        Assert.Equal(ErrorCodes.InvalidRedirectUri, result.Details.Title);
        Assert.Equal(Strings.TheRedirectionUrlIsNotTheSame, result.Details.Detail);
    }

    [Fact]
    public async Task When_The_Authorization_Code_Has_Expired_Then_Exception_Is_Thrown()
    {
        InitializeFakeObjects(TimeSpan.FromSeconds(2));
        var clientId = "clientId";
        var clientSecret = "clientSecret";
        var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
        {
            Code = "abc",
            ClientAssertion = "clientAssertion",
            ClientAssertionType = "clientAssertionType",
            ClientSecret = clientSecret,
            RedirectUri = new Uri("https://redirectUri"),
            ClientId = clientId,
        };
        var client = new Client
        {
            RequirePkce = false,
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.AuthorizationCode },
            ResponseTypes = new[] { ResponseTypeNames.Code }
        };
        var authorizationCode = new AuthorizationCode
        {
            ClientId = clientId,
            RedirectUri = new Uri("https://redirectUri"),
            CreateDateTime = DateTimeOffset.UtcNow.AddSeconds(-30)
        };

        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);
        _authorizationCodeStoreFake.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(authorizationCode);

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByAuthorizationCodeGrantTypeAction
            .Execute(
                authorizationCodeGrantTypeParameter,
                authenticationHeader,
                null,
                "",
                CancellationToken.None)
        );
        Assert.Equal(ErrorCodes.ExpiredAuthorizationCode, result.Details.Title);
        Assert.Equal(Strings.TheAuthorizationCodeIsObsolete, result.Details.Detail);
    }

    [Fact]
    public async Task When_RedirectUri_Is_Different_From_The_One_Hold_By_The_Client_Then_Error_Is_Returned()
    {
        InitializeFakeObjects(TimeSpan.FromSeconds(3000));
        var clientId = "clientId";
        var clientSecret = "clientSecret";
        var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
        {
            Code = "abc",
            ClientAssertion = "clientAssertion",
            ClientAssertionType = "clientAssertionType",
            ClientSecret = clientSecret,
            RedirectUri = new Uri("https://redirectUri"),
            ClientId = clientId,
        };
        var client = new Client
        {
            RequirePkce = false,
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.AuthorizationCode },
            ResponseTypes = new[] { ResponseTypeNames.Code }
        };
        var authorizationCode = new AuthorizationCode
        {
            ClientId = clientId,
            RedirectUri = new Uri("https://redirectUri"),
            CreateDateTime = DateTimeOffset.UtcNow
        };

        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);
        _authorizationCodeStoreFake.Remove(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        _authorizationCodeStoreFake.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(authorizationCode);

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var result = Assert.IsType<Option<GrantedToken>.Error>(await _getTokenByAuthorizationCodeGrantTypeAction
            .Execute(
                authorizationCodeGrantTypeParameter,
                authenticationHeader,
                null,
                "",
                CancellationToken.None)
        );
        Assert.Equal(ErrorCodes.InvalidRedirectUri, result.Details.Title);
        Assert.Equal(
            string.Format(Strings.RedirectUrlIsNotValid, "https://redirecturi/"),
            result.Details.Detail);
    }

    [Fact]
    public async Task When_Requesting_An_Existed_Granted_Token_Then_Check_Id_Token_Is_Signed_And_Encrypted()
    {
        InitializeFakeObjects(TimeSpan.FromSeconds(3000));
        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.CreateEncodedJwt(
            "test",
            "test",
            new ClaimsIdentity(),
            null,
            null,
            DateTime.Now,
            await _inMemoryJwksRepository.GetDefaultSigningKey());
        const string identityToken = "identityToken";
        const string clientId = "clientId";
        var clientSecret = "clientSecret";
        var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
        {
            ClientAssertion = "clientAssertion",
            ClientAssertionType = "clientAssertionType",
            ClientSecret = clientSecret,
            RedirectUri = new Uri("https://redirectUri"),
            ClientId = clientId
        };

        var client = new Client
        {
            AllowedScopes = new[] { "scope" },
            RedirectionUrls = new[] { new Uri("https://redirectUri") },
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.AuthorizationCode },
            ResponseTypes = new[] { ResponseTypeNames.Code },
            IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
            IdTokenEncryptedResponseAlg = SecurityAlgorithms.RsaPKCS1,
            IdTokenEncryptedResponseEnc = SecurityAlgorithms.Aes128CbcHmacSha256
        };
        var authorizationCode = new AuthorizationCode
        {
            ClientId = clientId,
            RedirectUri = new Uri("https://redirectUri"),
            CreateDateTime = DateTimeOffset.UtcNow,
            Scopes = "scope"
        };
        var grantedToken = new GrantedToken
        {
            ClientId = clientId,
            AccessToken = accessToken,
            IdToken = identityToken,
            IdTokenPayLoad = new JwtPayload(),
            CreateDateTime = DateTimeOffset.UtcNow,
            ExpiresIn = 100000
        };

        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        _authorizationCodeStoreFake.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(authorizationCode);

        _tokenStoreFake.GetToken(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<JwtPayload>(),
                Arg.Any<JwtPayload>(),
                Arg.Any<CancellationToken>())
            .Returns(grantedToken);
        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        var r = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
                authorizationCodeGrantTypeParameter,
                authenticationHeader,
                null,
                "",
                CancellationToken.None)
            ;

        Assert.NotNull(r);
    }

    [Fact]
    public async Task When_Requesting_Token_And_There_Is_No_Valid_Granted_Token_Then_Grant_A_New_One()
    {
        InitializeFakeObjects();

        const string clientId = "clientId";
        var clientSecret = "clientSecret";
        var authorizationCodeGrantTypeParameter = new AuthorizationCodeGrantTypeParameter
        {
            Code = "abc",
            ClientAssertion = "clientAssertion",
            ClientAssertionType = "clientAssertionType",
            ClientSecret = clientSecret,
            RedirectUri = new Uri("https://redirectUri"),
            ClientId = clientId
        };
        var client = new Client
        {
            RequirePkce = false,
            ClientName = clientId,
            RedirectionUrls = new[] { new Uri("https://redirectUri") },
            ClientId = clientId,
            Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = clientSecret } },
            GrantTypes = new[] { GrantTypes.AuthorizationCode },
            ResponseTypes = new[] { ResponseTypeNames.Code },
            JsonWebKeys =
                "supersecretlongkey".CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify)
                    .ToSet(),
            IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256
        };
        var authorizationCode = new AuthorizationCode
        {
            Scopes = "scope",
            ClientId = clientId,
            RedirectUri = new Uri("https://redirectUri"),
            CreateDateTime = DateTimeOffset.UtcNow
        };

        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);

        _authorizationCodeStoreFake.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(authorizationCode);
        _dotAuthOptions = new RuntimeSettings(authorizationCodeValidityPeriod: TimeSpan.FromSeconds(3000));

        _tokenStoreFake.GetToken(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<JwtPayload>(),
            Arg.Any<JwtPayload>(),
            Arg.Any<CancellationToken>()).ReturnsNull();

        var authenticationHeader = new AuthenticationHeaderValue(
            "Basic",
            $"{clientId}:{clientSecret}".Base64Encode());
        _ = await _getTokenByAuthorizationCodeGrantTypeAction.Execute(
            authorizationCodeGrantTypeParameter,
            authenticationHeader,
            null,
            "",
            CancellationToken.None);

        await _tokenStoreFake.Received().AddToken(Arg.Any<GrantedToken>(), Arg.Any<CancellationToken>());
        await _eventPublisher.Received().Publish(Arg.Any<TokenGranted>());
    }

    private void InitializeFakeObjects(TimeSpan authorizationCodeValidity = default)
    {
        _eventPublisher = Substitute.For<IEventPublisher>();
        _authorizationCodeStoreFake = Substitute.For<IAuthorizationCodeStore>();
        _tokenStoreFake = Substitute.For<ITokenStore>();
        _clientStore = Substitute.For<IClientStore>();
        _dotAuthOptions = new RuntimeSettings(
            authorizationCodeValidityPeriod: authorizationCodeValidity == default
                ? TimeSpan.FromSeconds(3600)
                : authorizationCodeValidity);
        _inMemoryJwksRepository = new InMemoryJwksRepository();
        _getTokenByAuthorizationCodeGrantTypeAction = new GetTokenByAuthorizationCodeGrantTypeAction(
            _authorizationCodeStoreFake,
            _dotAuthOptions,
            _clientStore,
            _eventPublisher,
            _tokenStoreFake,
            _inMemoryJwksRepository);
    }
}
