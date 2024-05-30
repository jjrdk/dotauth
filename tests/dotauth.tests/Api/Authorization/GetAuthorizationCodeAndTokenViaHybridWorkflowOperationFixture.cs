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
using NSubstitute;
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
                Substitute.For<IClientStore>(),
                Substitute.For<IConsentRepository>(),
                Substitute.For<IAuthorizationCodeStore>(),
                Substitute.For<ITokenStore>(),
                Substitute.For<IScopeRepository>(),
                new InMemoryJwksRepository(),
                new NoOpPublisher(),
                NullLogger.Instance);
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
            ;
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

        var client = new Client { RedirectionUrls = [redirectUrl], AllowedScopes = ["openid"], };
        var ex = await _getAuthorizationCodeAndTokenViaHybridWorkflowOperation.Execute(
                authorizationParameter,
                new ClaimsPrincipal(),
                client,
                "",
                CancellationToken.None)
            ;
        Assert.Equal(ActionResultType.BadRequest, ex.Type);
    }
}
