namespace DotAuth.Tests.WebSite.Authenticate;

using System;
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
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public sealed class AuthenticateHelperFixture
{
    private readonly IClientStore _clientRepositoryStub;
    private readonly AuthenticateHelper _authenticateHelper;
    private readonly IConsentRepository _consentRepository;

    public AuthenticateHelperFixture(ITestOutputHelper outputHelper)
    {
        _clientRepositoryStub = Substitute.For<IClientStore>();
        _consentRepository = Substitute.For<IConsentRepository>();
        var scopeRepository = Substitute.For<IScopeRepository>();
        scopeRepository.SearchByNames(Arg.Any<CancellationToken>(), Arg.Any<string[]>())
            .Returns([new Scope { Name = "scope" }]);
        _authenticateHelper = new AuthenticateHelper(
            Substitute.For<IAuthorizationCodeStore>(),
            Substitute.For<ITokenStore>(),
            scopeRepository,
            _consentRepository,
            _clientRepositoryStub,
            new InMemoryJwksRepository(),
            new NoopEventPublisher(),
            new TestOutputLogger("test", outputHelper));
    }

    [Fact]
    public async Task When_Client_Does_Not_Exist_Then_Exception_Is_Thrown()
    {
        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Client?)null);
        var authorizationParameter = new AuthorizationParameter { ClientId = "client_id" };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authenticateHelper.ProcessRedirection(
                authorizationParameter,
                null,
                "",
                [],
                "",
                CancellationToken.None));
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
        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Client());

        var actionResult = await _authenticateHelper.ProcessRedirection(
                authorizationParameter,
                code,
                subject,
                [],
                "",
                CancellationToken.None);

        Assert.Equal(DotAuthEndPoints.ConsentIndex, actionResult.RedirectInstruction!.Action);
        Assert.Contains(actionResult.RedirectInstruction.Parameters, p => p is { Name: code, Value: code });
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
        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Client());
        _clientRepositoryStub.GetAll(Arg.Any<CancellationToken>())
            .Returns([]);
        var consent = new Consent
        {
            GrantedScopes = ["scope"],
            ClientId = "client"
        };
        _consentRepository.GetConsentsForGivenUser(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns([consent]);

        var actionResult = await _authenticateHelper.ProcessRedirection(
                authorizationParameter,
                code,
                subject,
                [],
                "",
                CancellationToken.None);

        Assert.Equal(DotAuth.ResponseModes.FormPost, actionResult.RedirectInstruction!.ResponseMode);
    }

    [Fact]
    public async Task When_There_Is_No_Consent_Then_Redirect_To_Consent_Screen()
    {
        const string subject = "subject";
        const string code = "code";

        var authorizationParameter = new AuthorizationParameter { ClientId = "abc" };
        _clientRepositoryStub.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new Client());

        var actionResult = await _authenticateHelper.ProcessRedirection(
                authorizationParameter,
                code,
                subject,
                [],
                "",
                CancellationToken.None);

        Assert.Equal(DotAuthEndPoints.ConsentIndex, actionResult.RedirectInstruction!.Action);
        Assert.Contains(actionResult.RedirectInstruction.Parameters, p => p is { Name: code, Value: code });
    }
}
