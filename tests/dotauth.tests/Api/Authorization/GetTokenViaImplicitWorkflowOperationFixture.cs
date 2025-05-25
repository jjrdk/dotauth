namespace DotAuth.Tests.Api.Authorization;

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth;
using DotAuth.Api.Authorization;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Repositories;
using DotAuth.Results;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using Client = Shared.Models.Client;

public sealed class GetTokenViaImplicitWorkflowOperationFixture
{
    private readonly GetTokenViaImplicitWorkflowOperation _getTokenViaImplicitWorkflowOperation = new(
        Substitute.For<IClientStore>(),
        Substitute.For<IConsentRepository>(),
        Substitute.For<IAuthorizationCodeStore>(),
        Substitute.For<ITokenStore>(),
        Substitute.For<IScopeRepository>(),
        new InMemoryJwksRepository(),
        new NoOpPublisher(),
        NullLogger.Instance);

    [Fact]
    public async Task WhenPassingEmptyAuthorizationRequestThenErrorIsReturned()
    {
        var result = await _getTokenViaImplicitWorkflowOperation.Execute(
                new AuthorizationParameter(),
                new ClaimsPrincipal(),
                new Client(),
                "",
                CancellationToken.None);

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
            ;
        Assert.Equal(ErrorCodes.InvalidRequest, result.Error!.Title);
        Assert.Equal(
            string.Format(
                Strings.MissingParameter,
                DotAuth.CoreConstants.StandardAuthorizationRequestParameterNames.NonceName),
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
            CancellationToken.None);
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

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "test")], "fake"));

        var client = new Client
        {
            ResponseTypes = ResponseTypeNames.All,
            RedirectionUrls = [new Uri("https://localhost")],
            GrantTypes = [GrantTypes.Implicit],
            AllowedScopes = ["openid"]
        };
        var result = await _getTokenViaImplicitWorkflowOperation.Execute(
            authorizationParameter,
            claimsPrincipal,
            client,
            "",
            CancellationToken.None);

        Assert.NotNull(result.RedirectInstruction);
    }
}
