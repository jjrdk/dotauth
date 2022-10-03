﻿namespace DotAuth.Tests.WebSite.Authenticate;

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using DotAuth.Events;
using DotAuth.Parameters;
using DotAuth.Repositories;
using DotAuth.Results;
using DotAuth.Shared.Models;
using DotAuth.Shared.Properties;
using DotAuth.Shared.Repositories;
using DotAuth.WebSite.Authenticate;
using Moq;
using Xunit;
using Xunit.Abstractions;

public sealed class AuthenticateHelperFixture
{
    private readonly Mock<IClientStore> _clientRepositoryStub;
    private readonly AuthenticateHelper _authenticateHelper;
    private readonly Mock<IConsentRepository> _consentRepository;

    public AuthenticateHelperFixture(ITestOutputHelper outputHelper)
    {
        _clientRepositoryStub = new Mock<IClientStore>();
        _consentRepository = new Mock<IConsentRepository>();
        var scopeRepository = new Mock<IScopeRepository>();
        scopeRepository.Setup(x => x.SearchByNames(It.IsAny<CancellationToken>(), It.IsAny<string[]>()))
            .ReturnsAsync(new[] { new Scope { Name = "scope" } });
        _authenticateHelper = new AuthenticateHelper(
            new Mock<IAuthorizationCodeStore>().Object,
            new Mock<ITokenStore>().Object,
            scopeRepository.Object,
            _consentRepository.Object,
            _clientRepositoryStub.Object,
            new InMemoryJwksRepository(),
            new NoopEventPublisher(),
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
    {
        await Assert.ThrowsAsync<NullReferenceException>(
                () => _authenticateHelper.ProcessRedirection(null, null, null, null, null, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task When_Client_Does_Not_Exist_Then_Exception_Is_Thrown()
    {
        _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client)null);
        var authorizationParameter = new AuthorizationParameter { ClientId = "client_id" };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authenticateHelper.ProcessRedirection(
                    authorizationParameter,
                    null,
                    null,
                    null,
                    null,
                    CancellationToken.None))
            .ConfigureAwait(false);
        Assert.Equal(
            string.Format(SharedStrings.TheClientDoesntExist),
            exception.Message);
    }

    [Fact]
    public async Task When_PromptConsent_Parameter_Is_Passed_Then_Redirect_To_ConsentScreen()
    {
        const string subject = "subject";
        const string code = "code";

        var authorizationParameter = new AuthorizationParameter { ClientId = "abc" };
        _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client());
        //_parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
        //    .Returns(prompts);

        var actionResult = await _authenticateHelper.ProcessRedirection(
                authorizationParameter,
                code,
                subject,
                Array.Empty<Claim>(),
                null,
                CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(DotAuthEndPoints.ConsentIndex, actionResult.RedirectInstruction.Action);
        Assert.Contains(actionResult.RedirectInstruction.Parameters, p => p.Name == code && p.Value == code);
    }

    [Fact]
    public async Task When_Consent_Has_Already_Been_Given_Then_Redirect_To_Callback()
    {
        const string subject = "subject";
        const string code = "code";

        var authorizationParameter = new AuthorizationParameter
        {
            ClientId = "client",
            Scope = "scope",
            Prompt = "none",
            RedirectUrl = new Uri("https://localhost"),
            ResponseMode = DotAuth.ResponseModes.FormPost
        };
        _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client());
        _clientRepositoryStub.Setup(x => x.GetAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Client>());
        var consent = new Consent
        {
            GrantedScopes = new[] { "scope" },
            ClientId = "client"
        };
        _consentRepository.Setup(x => x.GetConsentsForGivenUser(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { consent });

        var actionResult = await _authenticateHelper.ProcessRedirection(
                authorizationParameter,
                code,
                subject,
                Array.Empty<Claim>(),
                null,
                CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(DotAuth.ResponseModes.FormPost, actionResult.RedirectInstruction.ResponseMode);
    }

    [Fact]
    public async Task When_There_Is_No_Consent_Then_Redirect_To_Consent_Screen()
    {
        const string subject = "subject";
        const string code = "code";

        var authorizationParameter = new AuthorizationParameter { ClientId = "abc" };
        _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Client());

        var actionResult = await _authenticateHelper.ProcessRedirection(
                authorizationParameter,
                code,
                subject,
                Array.Empty<Claim>(),
                null,
                CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(DotAuthEndPoints.ConsentIndex, actionResult.RedirectInstruction.Action);
        Assert.Contains(actionResult.RedirectInstruction.Parameters, p => p.Name == code && p.Value == code);
    }
}