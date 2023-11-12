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
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using Client = Shared.Models.Client;

public sealed class GetAuthorizationCodeOperationFixture
{
    private const string HttpsLocalhost = "https://localhost";
    private readonly GetAuthorizationCodeOperation _getAuthorizationCodeOperation = new(
        Substitute.For<IAuthorizationCodeStore>(),
        Substitute.For<ITokenStore>(),
        Substitute.For<IScopeRepository>(),
        Substitute.For<IClientStore>(),
        Substitute.For<IConsentRepository>(),
        new InMemoryJwksRepository(),
        new NoOpPublisher(),
        NullLogger.Instance);

    [Fact]
    public async Task WhenTheClientGrantTypeIsNotSupportedThenAnErrorIsReturned()
    {
        const string clientId = "clientId";
        const string scope = "scope";
        var authorizationParameter = new AuthorizationParameter
        {
            ResponseType = ResponseTypeNames.Code,
            RedirectUrl = new Uri(HttpsLocalhost),
            ClientId = clientId,
            Scope = scope,
            Claims = new ClaimsParameter()
        };

        var client = new Client
        {
            GrantTypes = new[] { GrantTypes.ClientCredentials },
            AllowedScopes = new[] { scope },
            RedirectionUrls = new[] { new Uri(HttpsLocalhost), }
        };
        var result = await _getAuthorizationCodeOperation.Execute(
                authorizationParameter,
                new ClaimsPrincipal(),
                client,
                "",
                CancellationToken.None)
            ;

        Assert.NotNull(result);
        Assert.Equal(ErrorCodes.InvalidRequest, result.Error!.Title);
        Assert.Equal(
            string.Format(Strings.TheClientDoesntSupportTheGrantType, clientId, "authorization_code"),
            result.Error.Detail);
    }

    [Fact]
    public async Task When_Passing_Valid_Request_Then_ReturnsRedirectInstruction()
    {
        const string clientId = "clientId";
        const string scope = "scope";

        var client = new Client
        {
            ResponseTypes = new[] { ResponseTypeNames.Code },
            AllowedScopes = new[] { scope },
            RedirectionUrls = new[] { new Uri(HttpsLocalhost), }
        };
        var authorizationParameter = new AuthorizationParameter
        {
            ResponseType = ResponseTypeNames.Code,
            RedirectUrl = new Uri(HttpsLocalhost),
            ClientId = clientId,
            Scope = scope,
            Claims = null
        };

        var result = await _getAuthorizationCodeOperation
            .Execute(authorizationParameter, new ClaimsPrincipal(), client, "", CancellationToken.None)
            ;

        Assert.NotNull(result.RedirectInstruction);
    }
}
