namespace DotAuth.Tests.WebSite.Consent;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.WebSite.Consent.Actions;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public sealed class DisplayConsentActionFixture
{
    private readonly IScopeRepository _scopeRepositoryFake;
    private readonly IClientStore _clientRepositoryFake;
    private readonly DisplayConsentAction _displayConsentAction;
    private readonly IConsentRepository _consentRepository;

    public DisplayConsentActionFixture(ITestOutputHelper outputHelper)
    {
        _scopeRepositoryFake = Substitute.For<IScopeRepository>();
        _clientRepositoryFake = Substitute.For<IClientStore>();
        _consentRepository = Substitute.For<IConsentRepository>();
        _displayConsentAction = new DisplayConsentAction(
            _scopeRepositoryFake,
            _clientRepositoryFake,
            _consentRepository,
            Substitute.For<IAuthorizationCodeStore>(),
            Substitute.For<ITokenStore>(),
            new InMemoryJwksRepository(),
            Substitute.For<IEventPublisher>(),
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_Parameter_Is_Empty_Then_Exception_Is_Thrown()
    {
        var authorizationParameter = new AuthorizationParameter();

        var error = await _displayConsentAction.Execute(
                authorizationParameter,
                new ClaimsPrincipal(),
                "",
                CancellationToken.None)
            ;

        Assert.NotNull(error.EndpointResult.Error);
    }

    [Fact]
    public async Task When_A_Consent_Has_Been_Given_Then_Redirect_To_Callback()
    {
        var scope = "scope";
        var clientid = "client";
        var client = new Client
        {
            JsonWebKeys =
                "verylongkeyfortesting".CreateJwk(
                        JsonWebKeyUseNames.Sig,
                        KeyOperations.Sign,
                        KeyOperations.Verify)
                    .ToSet(),
            IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256,
            ClientId = clientid,
            AllowedScopes = new[] { scope }
        };
        var consent = new Consent { ClientId = client.ClientId, GrantedScopes = new[] { scope } };
        _consentRepository.GetConsentsForGivenUser(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new List<Consent> { consent });
        _scopeRepositoryFake.SearchByNames(Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns(new[] { new Scope { Name = scope, IsDisplayedInConsent = true } });
        var claimsIdentity = new ClaimsIdentity(new[] { new Claim("sub", "test"), }, "test");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        var authorizationParameter = new AuthorizationParameter
        {
            ClientId = clientid,
            Scope = scope,
            ResponseMode = DotAuth.ResponseModes.Fragment
        };

        _clientRepositoryFake.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);
        _clientRepositoryFake.GetAll(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Client>());
        var result = await _displayConsentAction
            .Execute(authorizationParameter, claimsPrincipal, null, CancellationToken.None)
            ;

        Assert.Equal(DotAuth.ResponseModes.Fragment, result.EndpointResult.RedirectInstruction.ResponseMode);
    }

    [Fact]
    public async Task
        When_A_Consent_Has_Been_Given_And_The_AuthorizationFlow_Is_Not_Supported_Then_Exception_Is_Thrown()
    {
        const string clientId = "clientId";
        const string state = "state";
        var claimsIdentity = new ClaimsIdentity(new[] { new Claim("sub", "test") }, "test");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var authorizationParameter = new AuthorizationParameter
        {
            ResponseType = ResponseTypeNames.Token,
            Scope = "scope",
            ClientId = clientId,
            State = state,
            ResponseMode = DotAuth.ResponseModes.None // No response mode is defined
        };
        var consent = new Consent
        {
            GrantedScopes = new[] { "scope" },
            ClientId = clientId
        };
        var returnedClient = new Client
        {
            ClientId = clientId,
            JsonWebKeys = "verylongkeyfortesting".CreateJwk(
                    JsonWebKeyUseNames.Sig,
                    KeyOperations.Sign,
                    KeyOperations.Verify)
                .ToSet(),
            IdTokenSignedResponseAlg = SecurityAlgorithms.RsaSha256
        };
        _clientRepositoryFake.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(returnedClient);
        _clientRepositoryFake.GetAll(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Client>());
        _consentRepository.GetConsentsForGivenUser(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new[] { consent });
        var result = await _displayConsentAction.Execute(
                authorizationParameter,
                claimsPrincipal,
                "issuer",
                CancellationToken.None)
            ;

        Assert.Equal(ErrorCodes.InvalidRequest, result.EndpointResult.Error.Title);
        Assert.Equal(Strings.TheAuthorizationFlowIsNotSupported, result.EndpointResult.Error.Detail);

    }

    [Fact]
    public async Task When_No_Consent_Has_Been_Given_And_Client_Does_Not_Exist_Then_Exception_Is_Thrown()
    {
        const string clientId = "clientId";
        const string state = "state";
        var claimsIdentity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var authorizationParameter = new AuthorizationParameter { ClientId = clientId, State = state };

        _clientRepositoryFake.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Client)null);

        var result = await _displayConsentAction.Execute(
                authorizationParameter,
                claimsPrincipal,
                null,
                CancellationToken.None)
            ;
        Assert.Equal(ErrorCodes.InvalidRequest, result.EndpointResult.Error.Title);
        Assert.Equal(string.Format(Strings.ClientIsNotValid, clientId), result.EndpointResult.Error.Detail);
    }

    [Fact]
    public async Task When_No_Consent_Has_Been_Given_Then_Redirect_To_Consent_Screen()
    {
        const string clientId = "clientId";
        const string state = "state";
        const string scopeName = "profile";
        var claimsIdentity = new ClaimsIdentity(new[] { new Claim("sub", "test") });
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var client = new Client();
        var authorizationParameter = new AuthorizationParameter
        {
            ClientId = clientId,
            State = state,
            Claims = null,
            Scope = scopeName
        };
        var scopes = new[] { new Scope { IsDisplayedInConsent = true, Name = scopeName } };

        _clientRepositoryFake.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);
        _scopeRepositoryFake.SearchByNames(Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns(scopes);

        await _displayConsentAction.Execute(authorizationParameter, claimsPrincipal, null, CancellationToken.None)
            ;

        Assert.Contains(scopes, s => s.Name == scopeName);
    }
}
