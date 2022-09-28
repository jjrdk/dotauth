namespace SimpleAuth.Tests.Api.Authorization;

using Moq;
using Parameters;
using Shared;
using Shared.Repositories;
using SimpleAuth;
using SimpleAuth.Api.Authorization;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using SimpleAuth.Properties;
using SimpleAuth.Repositories;
using SimpleAuth.Results;
using SimpleAuth.Shared.Errors;
using Xunit;
using Client = Shared.Models.Client;

public sealed class GetTokenViaImplicitWorkflowOperationFixture
{
    private readonly GetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation;

    public GetTokenViaImplicitWorkflowOperationFixture()
    {
        _getTokenViaImplicitWorkflowOperation = new GetTokenViaImplicitWorkflowOperation(
            new Mock<IClientStore>().Object,
            new Mock<IConsentRepository>().Object,
            new Mock<IAuthorizationCodeStore>().Object,
            new Mock<ITokenStore>().Object,
            new Mock<IScopeRepository>().Object,
            new InMemoryJwksRepository(),
            new NoOpPublisher(),
            NullLogger.Instance);
    }

    [Fact]
    public async Task When_Passing_No_Authorization_Request_Then_Exception_Is_Thrown()
    {
        await Assert.ThrowsAsync<NullReferenceException>(
                () => _getTokenViaImplicitWorkflowOperation.Execute(null, null, null, null, CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task WhenPassingEmptyAuthorizationRequestThenErrorIsReturned()
    {
        var result = await _getTokenViaImplicitWorkflowOperation.Execute(
                new AuthorizationParameter(),
                new ClaimsPrincipal(),
                new Client(),
                "",
                CancellationToken.None)
            .ConfigureAwait(false);

        Assert.Equal(ActionResultType.BadRequest, result.Type);
    }

    [Fact]
    public async Task WhenPassingNoNonceParameterThenExceptionIsThrown()
    {
        var authorizationParameter = new AuthorizationParameter { State = "state" };

        var result = await _getTokenViaImplicitWorkflowOperation.Execute(
                authorizationParameter,
                new ClaimsPrincipal(),
                new Client(),
                "",
                CancellationToken.None)
            .ConfigureAwait(false);
        Assert.Equal(ErrorCodes.InvalidRequest, result.Error!.Title);
        Assert.Equal(
            string.Format(
                Strings.MissingParameter,
                CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
            result.Error.Detail);
    }

    [Fact]
    public async Task WhenImplicitFlowIsNotSupportedThenErrorIsReturned()
    {
        var authorizationParameter = new AuthorizationParameter { Nonce = "nonce", State = "state" };

        var ex = await _getTokenViaImplicitWorkflowOperation.Execute(
                authorizationParameter,
                new ClaimsPrincipal(),
                new Client(),
                "",
                CancellationToken.None)
            .ConfigureAwait(false);
        Assert.Equal(ErrorCodes.InvalidRequest, ex.Error!.Title);
        Assert.Equal(
            string.Format(Strings.TheClientDoesntSupportTheGrantType, authorizationParameter.ClientId, "implicit"),
            ex.Error!.Detail);
    }

    [Fact]
    public async Task When_Requesting_Authorization_With_Valid_Request_Then_ReturnsRedirectInstruction()
    {
        const string clientId = "clientId";
        const string scope = "openid";
        var authorizationParameter = new AuthorizationParameter
        {
            ResponseType = ResponseTypeNames.Token,
            State = "state",
            Nonce = "nonce",
            ClientId = clientId,
            Scope = scope,
            Claims = null,
            RedirectUrl = new Uri("https://localhost")
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "test") }, "fake"));

        var client = new Client
        {
            ResponseTypes = ResponseTypeNames.All,
            RedirectionUrls = new[] { new Uri("https://localhost"), },
            GrantTypes = new[] { GrantTypes.Implicit },
            AllowedScopes = new[] { "openid" }
        };
        var result = await _getTokenViaImplicitWorkflowOperation.Execute(
                authorizationParameter,
                claimsPrincipal,
                client,
                null,
                CancellationToken.None)
            .ConfigureAwait(false);

        Assert.NotNull(result.RedirectInstruction);
    }
}