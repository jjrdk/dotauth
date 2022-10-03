namespace DotAuth.Tests.Api.Authorization;

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth;
using DotAuth.Api.Authorization;
using DotAuth.Parameters;
using DotAuth.Repositories;
using DotAuth.Results;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

//using Client = Shared.Models.Client;

public sealed class GetAuthorizationCodeAndTokenViaHybridWorkflowOperationFixture
{
    private readonly GetAuthorizationCodeAndTokenViaHybridWorkflowOperation
        _getAuthorizationCodeAndTokenViaHybridWorkflowOperation;

    public GetAuthorizationCodeAndTokenViaHybridWorkflowOperationFixture()
    {
        _getAuthorizationCodeAndTokenViaHybridWorkflowOperation =
            new GetAuthorizationCodeAndTokenViaHybridWorkflowOperation(
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
    public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
    {
        await Assert.ThrowsAsync<NullReferenceException>(
                () => _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(
                    null,
                    null,
                    null,
                    null,
                    CancellationToken.None))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task WhenNonceParameterIsNotSetThenAnErrorIsReturned()
    {
        var authorizationParameter = new AuthorizationParameter { State = "state" };

        var ex = await _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(
                authorizationParameter,
                new ClaimsPrincipal(),
                new Client(),
                "",
                CancellationToken.None)
            .ConfigureAwait(false);
        Assert.Equal(ActionResultType.BadRequest, ex.Type);
    }

    [Fact]
    public async Task WhenGrantTypeIsNotSupportedThenAnErrorIsReturned()
    {
        var redirectUrl = new Uri("https://localhost");
        var authorizationParameter = new AuthorizationParameter
        {
            RedirectUrl = redirectUrl,
            State = "state",
            Nonce = "nonce",
            Scope = "openid",
            ResponseType = ResponseTypeNames.Code,
        };

        var client = new Client { RedirectionUrls = new[] { redirectUrl }, AllowedScopes = new[] { "openid" }, };
        var ex = await _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(
                authorizationParameter,
                new ClaimsPrincipal(),
                client,
                null,
                CancellationToken.None)
            .ConfigureAwait(false);
        Assert.Equal(ActionResultType.BadRequest, ex.Type);
    }
}