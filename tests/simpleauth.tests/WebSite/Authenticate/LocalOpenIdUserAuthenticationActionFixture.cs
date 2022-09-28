namespace SimpleAuth.Tests.WebSite.Authenticate;

using Moq;
using Parameters;
using Shared;
using Shared.Models;
using SimpleAuth.Shared.Repositories;
using SimpleAuth.WebSite.Authenticate;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using SimpleAuth.Repositories;
using SimpleAuth.Services;
using Xunit;
using Xunit.Abstractions;

public sealed class LocalOpenIdUserAuthenticationActionFixture
{
    private readonly ITestOutputHelper _outputHelper;
    private LocalOpenIdUserAuthenticationAction _localUserAuthenticationAction;

    public LocalOpenIdUserAuthenticationActionFixture(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        InitializeFakeObjects();
    }
        
    [Fact]
    public async Task When_Resource_Owner_Cannot_Be_Authenticated_Then_Error_Message_Is_Returned()
    {
        var authenticateService = new Mock<IAuthenticateResourceOwnerService>();
        authenticateService.SetupGet(x => x.Amr).Returns("pwd");
        authenticateService
            .Setup(
                x => x.AuthenticateResourceOwner(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((ResourceOwner) null);
        InitializeFakeObjects(authenticateService.Object);
        var localAuthenticationParameter = new LocalAuthenticationParameter();
        var authorizationParameter = new AuthorizationParameter();

        var result = await _localUserAuthenticationAction!.Execute(
                localAuthenticationParameter,
                authorizationParameter,
                "",
                "",
                CancellationToken.None)
            .ConfigureAwait(false);

        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task When_Resource_Owner_Credentials_Are_Correct_Then_Event_Is_Logged_And_Claims_Are_Returned()
    {
        const string subject = "subject";
        var localAuthenticationParameter = new LocalAuthenticationParameter {Password = "abc", UserName = subject};
        var authorizationParameter = new AuthorizationParameter {ClientId = "client"};
        var resourceOwner = new ResourceOwner {Subject = subject};
        var authenticateService = new Mock<IAuthenticateResourceOwnerService>();
        authenticateService.SetupGet(x => x.Amr).Returns("pwd");
        authenticateService
            .Setup(
                x => x.AuthenticateResourceOwner(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(resourceOwner);
        InitializeFakeObjects(authenticateService.Object);

        var result = await _localUserAuthenticationAction!.Execute(
                localAuthenticationParameter,
                authorizationParameter,
                "",
                "",
                CancellationToken.None)
            .ConfigureAwait(false);

        // Specify the resource owner authentication date
        Assert.NotNull(result.Claims);
        Assert.Contains(
            result.Claims,
            r => r.Type == ClaimTypes.AuthenticationInstant || r.Type == OpenIdClaimTypes.Subject);
    }

    private void InitializeFakeObjects(params IAuthenticateResourceOwnerService[] services)
    {
        var mock = new Mock<IClientStore>();
        mock.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new Client());
        _localUserAuthenticationAction = new LocalOpenIdUserAuthenticationAction(
            new Mock<IAuthorizationCodeStore>().Object,
            services,
            new Mock<IConsentRepository>().Object,
            new Mock<ITokenStore>().Object,
            new Mock<IScopeRepository>().Object,
            mock.Object,
            new InMemoryJwksRepository(),
            new NoOpPublisher(),
            new TestOutputLogger("test", _outputHelper));
    }
}