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

namespace DotAuth.Tests.Common;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using DotAuth.Common;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Repositories;
using DotAuth.Results;
using DotAuth.Shared;
using DotAuth.Shared.Events.OAuth;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Tests.Helpers;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public sealed class GenerateAuthorizationResponseFixture
{
    private readonly IAuthorizationCodeStore _authorizationCodeRepositoryFake;
    private readonly ITokenStore _tokenStore;
    private readonly IEventPublisher _eventPublisher;
    private readonly GenerateAuthorizationResponse _generateAuthorizationResponse;
    private readonly IClientStore _clientStore;
    private readonly IConsentRepository _consentRepository;
    private readonly InMemoryJwksRepository _inMemoryJwksRepository;

    public GenerateAuthorizationResponseFixture(ITestOutputHelper outputHelper)
    {
        IdentityModelEventSource.ShowPII = true;
        _authorizationCodeRepositoryFake = Substitute.For<IAuthorizationCodeStore>();
        _tokenStore = Substitute.For<ITokenStore>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _eventPublisher.Publish(Arg.Any<TokenGranted>()).Returns(Task.CompletedTask);
        _clientStore = Substitute.For<IClientStore>();
        _clientStore.GetAll(Arg.Any<CancellationToken>()).Returns(Array.Empty<Client>());

        _consentRepository = Substitute.For<IConsentRepository>();
        var scopeRepository = Substitute.For<IScopeRepository>();
        scopeRepository.SearchByNames(Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns(new[] { new Scope { Name = "openid" } });
        _inMemoryJwksRepository = new InMemoryJwksRepository();
        _generateAuthorizationResponse = new GenerateAuthorizationResponse(
            _authorizationCodeRepositoryFake,
            _tokenStore,
            scopeRepository,
            _clientStore,
            _consentRepository,
            _inMemoryJwksRepository,
            _eventPublisher,
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_There_Is_No_Logged_User_Then_Exception_Is_Throw()
    {
        var redirectInstruction = new EndpointResult { RedirectInstruction = new RedirectInstruction() };

        await Assert.ThrowsAsync<ArgumentNullException>(
                () => _generateAuthorizationResponse.Generate(
                    redirectInstruction,
                    new AuthorizationParameter(),
                    new ClaimsPrincipal(),
                    new Client(),
                    "",
                    CancellationToken.None))
            ;
    }

    [Fact]
    public async Task When_Generating_AuthorizationResponse_With_IdToken_Then_IdToken_Is_Added_To_The_Parameters()
    {
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
        // const string idToken = "idToken";
        var clientId = "client";
        var authorizationParameter =
            new AuthorizationParameter { ResponseType = ResponseTypeNames.IdToken, ClientId = clientId };
        var actionResult = new EndpointResult { RedirectInstruction = new RedirectInstruction() };

        var client = new Client
        {
            ClientId = clientId,
            JsonWebKeys =
                TestKeys.SecretKey.CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify)
                    .ToSet(),
            IdTokenSignedResponseAlg = SecurityAlgorithms.HmacSha256
        };
        _clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(client);
        actionResult = await _generateAuthorizationResponse.Generate(
                actionResult,
                authorizationParameter,
                claimsPrincipal,
                client,
                "",
                CancellationToken.None)
            ;

        Assert.Contains(
            actionResult.RedirectInstruction!.Parameters,
            p => p.Name == StandardAuthorizationResponseNames.IdTokenName);
    }

    [Fact]
    public async Task
        When_Generating_AuthorizationResponse_With_AccessToken_And_ThereIs_No_Granted_Token_Then_Token_Is_Generated_And_Added_To_The_Parameters()
    {
        //const string idToken = "idToken";
        const string clientId = "clientId";
        const string scope = "openid";
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
        var authorizationParameter = new AuthorizationParameter
        {
            ResponseType = ResponseTypeNames.Token,
            ClientId = clientId,
            Scope = scope
        };

        var client = new Client
        {
            ClientId = clientId,
            JsonWebKeys =
                "supersecretlongkey".CreateJwk(JsonWebKeyUseNames.Sig, KeyOperations.Sign, KeyOperations.Verify)
                    .ToSet(),
            IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256
        };

        var actionResult = await _generateAuthorizationResponse.Generate(
                new EndpointResult { RedirectInstruction = new RedirectInstruction() },
                authorizationParameter,
                claimsPrincipal,
                client,
                "issuer",
                CancellationToken.None)
            ;

        Assert.Contains(
            actionResult.RedirectInstruction!.Parameters,
            p => p.Name == DotAuth.StandardAuthorizationResponseNames.AccessTokenName);
        await _tokenStore.Received().AddToken(Arg.Any<GrantedToken>(), Arg.Any<CancellationToken>());
        await _eventPublisher.Received().Publish(Arg.Any<TokenGranted>());
    }

    [Fact]
    public async Task
        When_Generating_AuthorizationResponse_With_AccessToken_And_ThereIs_A_GrantedToken_Then_Token_Is_Added_To_The_Parameters()
    {
        const string clientId = "clientId";
        const string scope = "openid";
        var claimsIdentity = new ClaimsIdentity("fake");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var authorizationParameter = new AuthorizationParameter
        {
            ResponseType = ResponseTypeNames.Token,
            ClientId = clientId,
            Scope = scope
        };
        var handler = new JwtSecurityTokenHandler();
        var issuedAt = DateTime.UtcNow;
        const int expiresIn = 20000;
        var defaultSigningKey = await _inMemoryJwksRepository.GetDefaultSigningKey();
        var accessToken = handler.CreateEncodedJwt(
            "test",
            clientId,
            claimsIdentity,
            null,
            issuedAt.AddSeconds(expiresIn),
            issuedAt,
            defaultSigningKey);
        var grantedToken = new GrantedToken
        {
            ClientId = clientId,
            AccessToken = accessToken,
            CreateDateTime = issuedAt,
            ExpiresIn = expiresIn
        };

        _tokenStore.GetToken(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<JwtPayload>(),
                Arg.Any<JwtPayload>(),
                Arg.Any<CancellationToken>())
            .Returns(grantedToken);

        var actionResult = await _generateAuthorizationResponse.Generate(
                new EndpointResult { RedirectInstruction = new RedirectInstruction() },
                authorizationParameter,
                claimsPrincipal,
                new Client { ClientId = clientId },
                "test",
                CancellationToken.None)
            ;

        Assert.Equal(
            grantedToken.AccessToken,
            actionResult.RedirectInstruction!.Parameters
                .First(x => x.Name == DotAuth.StandardAuthorizationResponseNames.AccessTokenName)
                .Value);
    }

    [Fact]
    public async Task
        When_Generating_AuthorizationResponse_With_AuthorizationCode_Then_Code_Is_Added_To_The_Parameters()
    {
        //const string idToken = "idToken";
        const string clientId = "clientId";
        const string scope = "openid";
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "test"), }, "fake"));
        var authorizationParameter = new AuthorizationParameter
        {
            RedirectUrl = new Uri("http://localhost"),
            ResponseType = ResponseTypeNames.Code,
            ClientId = clientId,
            Scope = scope
        };

        var consent = new Consent
        {
            GrantedScopes = new[] { scope },
            ClientId = clientId
        };

        _consentRepository.GetConsentsForGivenUser(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { consent });

        var actionResult = await _generateAuthorizationResponse.Generate(
                new EndpointResult { RedirectInstruction = new RedirectInstruction() },
                authorizationParameter,
                claimsPrincipal,
                new Client(),
                "",
                CancellationToken.None);

        Assert.Contains(
            actionResult.RedirectInstruction!.Parameters,
            p => p.Name == DotAuth.StandardAuthorizationResponseNames.AuthorizationCodeName);
        await _authorizationCodeRepositoryFake.Received()
            .Add(Arg.Any<AuthorizationCode>(), Arg.Any<CancellationToken>());
        await _eventPublisher.Received().Publish(Arg.Any<AuthorizationGranted>());
    }

    [Fact]
    public async Task
        When_Redirecting_To_Callback_And_There_Is_No_Response_Mode_Specified_Then_The_Response_Mode_Is_Set()
    {
        //const string idToken = "idToken";
        const string clientId = "clientId";
        const string scope = "scope";
        const string responseType = "id_token";
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity("fake"));
        var authorizationParameter = new AuthorizationParameter
        {
            ClientId = clientId,
            Scope = scope,
            ResponseType = responseType,
            ResponseMode = DotAuth.ResponseModes.None
        };

        var actionResult = await _generateAuthorizationResponse.Generate(
                new EndpointResult
                {
                    RedirectInstruction = new RedirectInstruction(),
                    Type = ActionResultType.RedirectToCallBackUrl
                },
                authorizationParameter,
                claimsPrincipal,
                new Client(),
                "",
                CancellationToken.None);

        Assert.Equal(DotAuth.ResponseModes.Fragment, actionResult.RedirectInstruction!.ResponseMode);
    }
}
