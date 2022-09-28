﻿namespace SimpleAuth.Tests.WebSite.Consent;

using Moq;
using Parameters;
using Shared;
using Shared.Models;
using Shared.Repositories;
using SimpleAuth.WebSite.Consent.Actions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using SimpleAuth.Properties;
using SimpleAuth.Repositories;
using SimpleAuth.Shared.Errors;
using Xunit;
using Xunit.Abstractions;

public sealed class ConfirmConsentFixture
{
    private readonly Mock<IConsentRepository> _consentRepositoryFake;
    private readonly Mock<IClientStore> _clientRepositoryFake;
    private readonly Mock<IScopeRepository> _scopeRepositoryFake;
    private readonly ConfirmConsentAction _confirmConsentAction;

    public ConfirmConsentFixture(ITestOutputHelper outputHelper)
    {
        _consentRepositoryFake = new Mock<IConsentRepository>();
        _clientRepositoryFake = new Mock<IClientStore>();
        _scopeRepositoryFake = new Mock<IScopeRepository>();
        _confirmConsentAction = new ConfirmConsentAction(
            new Mock<IAuthorizationCodeStore>().Object,
            new Mock<ITokenStore>().Object,
            _consentRepositoryFake.Object,
            _clientRepositoryFake.Object,
            _scopeRepositoryFake.Object,
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
            ResponseMode = ResponseModes.None,
            State = state
        };
        var claims = new List<Claim> { new(OpenIdClaimTypes.Subject, subject) };
        var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuthServer");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var client = new Client { ClientId = "clientId" };

        _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _clientRepositoryFake.Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Client>());

        _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(Array.Empty<Scope>());
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
                    new ClaimParameter {Name = OpenIdClaimTypes.Subject}
                }
            },
            Scope = "profile"
        };
        var claims = new List<Claim> { new(OpenIdClaimTypes.Subject, subject) };
        var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuthServer");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var client = new Client { ClientId = clientId };

        _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _clientRepositoryFake.Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Client>());
        _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(Array.Empty<Scope>());

        Consent insertedConsent = null;
        _consentRepositoryFake.Setup(co => co.Insert(It.IsAny<Consent>(), It.IsAny<CancellationToken>()))
            .Callback<Consent, CancellationToken>((consent, token) => insertedConsent = consent)
            .ReturnsAsync(true);

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
            ResponseMode = ResponseModes.None
        };
        var claims = new List<Claim> { new(OpenIdClaimTypes.Subject, subject) };
        var claimsIdentity = new ClaimsIdentity(claims, "SimpleAuthServer");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var client = new Client { ClientId = "clientId" };
        _clientRepositoryFake.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        _clientRepositoryFake.Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Client>());
        _scopeRepositoryFake.Setup(s => s.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(Array.Empty<Scope>());

        var result = await _confirmConsentAction
            .Execute(authorizationParameter, claimsPrincipal, "null", CancellationToken.None)
            .ConfigureAwait(false);

        _consentRepositoryFake.Verify(c => c.Insert(It.IsAny<Consent>(), It.IsAny<CancellationToken>()));
        Assert.Equal(ResponseModes.Query, result.RedirectInstruction!.ResponseMode);
    }
}