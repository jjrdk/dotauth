namespace DotAuth.Tests.WebSite.Consent;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using DotAuth;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Repositories;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.WebSite.Consent.Actions;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public sealed class ConfirmConsentFixture
{
    private readonly IConsentRepository _consentRepositoryFake;
    private readonly IClientStore _clientRepositoryFake;
    private readonly IScopeRepository _scopeRepositoryFake;
    private readonly ConfirmConsentAction _confirmConsentAction;

    public ConfirmConsentFixture(ITestOutputHelper outputHelper)
    {
        _consentRepositoryFake = Substitute.For<IConsentRepository>();
        _clientRepositoryFake = Substitute.For<IClientStore>();
        _scopeRepositoryFake = Substitute.For<IScopeRepository>();
        _confirmConsentAction = new ConfirmConsentAction(
            Substitute.For<IAuthorizationCodeStore>(),
            Substitute.For<ITokenStore>(),
            _consentRepositoryFake,
            _clientRepositoryFake,
            _scopeRepositoryFake,
            new InMemoryJwksRepository(),
            new NoOpPublisher(),
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_No_Consent_Has_Been_Given_And_ResponseMode_Is_No_Correct_Then_Exception_Is_Thrown()
    {
        const string subject = "subject";
        const string state = "state";
        var authorizationParameter = new AuthorizationParameter
        {
            ClientId = "clientId",
            Claims = null,
            Scope = "profile",
            ResponseMode = DotAuth.ResponseModes.None,
            State = state
        };
        var claims = new List<Claim> { new(OpenIdClaimTypes.Subject, subject) };
        var claimsIdentity = new ClaimsIdentity(claims, "DotAuthServer");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var client = new Client { ClientId = "clientId" };

        _clientRepositoryFake.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);
        _clientRepositoryFake.GetAll(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Client>());

        _scopeRepositoryFake.SearchByNames(Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns(Array.Empty<Scope>());
        var exception = await _confirmConsentAction.Execute(
                authorizationParameter,
                claimsPrincipal,
                "null",
                CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(ErrorCodes.InvalidRequest, exception.Error!.Title);
        Assert.Equal(Strings.TheAuthorizationFlowIsNotSupported, exception.Error.Detail);
    }

    [Fact]
    public async Task When_No_Consent_Has_Been_Given_For_The_Claims_Then_Create_And_Insert_A_New_One()
    {
        const string subject = "subject";
        const string clientId = "clientId";
        var authorizationParameter = new AuthorizationParameter
        {
            ClientId = clientId,
            ResponseType = "code",
            Claims = new ClaimsParameter
            {
                UserInfo = new[]
                {
                    new ClaimParameter { Name = OpenIdClaimTypes.Subject }
                }
            },
            Scope = "profile"
        };
        var claims = new List<Claim> { new(OpenIdClaimTypes.Subject, subject) };
        var claimsIdentity = new ClaimsIdentity(claims, "DotAuthServer");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var client = new Client { ClientId = clientId };

        _clientRepositoryFake.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);
        _clientRepositoryFake.GetAll(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Client>());
        _scopeRepositoryFake.SearchByNames(Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns(Array.Empty<Scope>());

        Consent insertedConsent = null;
        _consentRepositoryFake.Insert(Arg.Any<Consent>(), Arg.Any<CancellationToken>())
            .Returns(true)
            .AndDoes(c => insertedConsent = c.Arg<Consent>());

        await _confirmConsentAction.Execute(authorizationParameter, claimsPrincipal, "null", CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Contains(OpenIdClaimTypes.Subject, insertedConsent!.Claims);
        Assert.Equal(subject, insertedConsent.Subject);
        Assert.Equal(clientId, insertedConsent.ClientId);
    }

    [Fact]
    public async Task When_No_Consent_Has_Been_Given_Then_Create_And_Insert_A_New_One()
    {
        const string subject = "subject";
        var authorizationParameter = new AuthorizationParameter
        {
            ClientId = "clientId",
            ResponseType = "code",
            Claims = null,
            Scope = "profile",
            ResponseMode = DotAuth.ResponseModes.None
        };
        var claims = new List<Claim> { new(OpenIdClaimTypes.Subject, subject) };
        var claimsIdentity = new ClaimsIdentity(claims, "DotAuthServer");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var client = new Client { ClientId = "clientId" };
        _clientRepositoryFake.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(client);
        _clientRepositoryFake.GetAll(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Client>());
        _scopeRepositoryFake.SearchByNames(Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns(Array.Empty<Scope>());

        var result = await _confirmConsentAction
            .Execute(authorizationParameter, claimsPrincipal, "null", CancellationToken.None)
            .ConfigureAwait(false);

        await _consentRepositoryFake.Received().Insert(Arg.Any<Consent>(), Arg.Any<CancellationToken>());
        Assert.Equal(DotAuth.ResponseModes.Query, result.RedirectInstruction!.ResponseMode);
    }
}
