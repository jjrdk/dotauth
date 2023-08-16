namespace DotAuth.Tests.WebSite.Authenticate;

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Divergic.Logging.Xunit;
using DotAuth;
using DotAuth.Parameters;
using DotAuth.Repositories;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.WebSite.Authenticate;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
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
        var authenticateService = Substitute.For<IAuthenticateResourceOwnerService>();
        authenticateService.Amr.Returns("pwd");
        authenticateService.AuthenticateResourceOwner(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .ReturnsNull();
        InitializeFakeObjects(authenticateService);
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
        var authenticateService = Substitute.For<IAuthenticateResourceOwnerService>();
        authenticateService.Amr.Returns("pwd");
        authenticateService.AuthenticateResourceOwner(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<CancellationToken>())
            .Returns(resourceOwner);
        InitializeFakeObjects(authenticateService);

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
        var mock = Substitute.For<IClientStore>();
        mock.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new Client());
        _localUserAuthenticationAction = new LocalOpenIdUserAuthenticationAction(
            Substitute.For<IAuthorizationCodeStore>(),
            services,
            Substitute.For<IConsentRepository>(),
            Substitute.For<ITokenStore>(),
            Substitute.For<IScopeRepository>(),
            mock,
            new InMemoryJwksRepository(),
            new NoOpPublisher(),
            new TestOutputLogger("test", _outputHelper));
    }
}
