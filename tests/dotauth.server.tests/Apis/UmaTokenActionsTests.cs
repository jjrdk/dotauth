namespace DotAuth.Server.Tests.Apis;

using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Token;
using DotAuth.Events;
using DotAuth.Parameters;
using DotAuth.Policies;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Policies;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Responses;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

public class UmaTokenActionsTests
{
    private readonly UmaTokenActions _tokenActions;

    public UmaTokenActionsTests()
    {
        var ticketStore = Substitute.For<ITicketStore>();
        ticketStore.Get(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                new Ticket
                {
                    Created = DateTimeOffset.UtcNow,
                    Expires = DateTimeOffset.MaxValue,
                    Id = "ticket",
                    IsAuthorizedByRo = false,
                    Lines = Array.Empty<TicketLine>(),
                    Requester = Array.Empty<ClaimData>(),
                    ResourceOwner = "ro"
                });
        ticketStore.Remove(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var clientStore = Substitute.For<IClientStore>();
        clientStore.GetById(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(
                new Client
                {
                    ClientId = "test_client",
                    GrantTypes = new[] { GrantTypes.UmaTicket },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" } }
                });
        var scopeStore = Substitute.For<IScopeStore>();
        var tokenStore = Substitute.For<ITokenStore>();
        tokenStore.AddToken(Arg.Any<GrantedToken>(), Arg.Any<CancellationToken>()).Returns(true);
        var authorizationPolicyValidator = Substitute.For<IAuthorizationPolicyValidator>();
        authorizationPolicyValidator.IsAuthorized(
                    Arg.Any<Ticket>(),
                    Arg.Any<Client>(),
                    Arg.Any<ClaimTokenParameter>(),
                    Arg.Any<CancellationToken>())
            .Returns(
                new AuthorizationPolicyResult(AuthorizationPolicyResultKind.Authorized, Array.Empty<Claim>()));
        var jwksStore = Substitute.For<IJwksStore>();
        var eventPublisher = Substitute.For<IEventPublisher>();
        _tokenActions = new UmaTokenActions(
            ticketStore,
            new RuntimeSettings(),
            clientStore,
            scopeStore,
            tokenStore,
            jwksStore,
            authorizationPolicyValidator,
            eventPublisher,
            NullLogger.Instance);
    }

    [Fact]
    public async Task CanGenerateTokenFromValidTicketRequest()
    {
        var option = await _tokenActions.GetTokenByTicketId(
            new GetTokenViaTicketIdParameter
            {
                Ticket = "ticket_id", ClientId = "client", ClientSecret = "secret",
                ClaimToken = new ClaimTokenParameter { Format = "", Token = "token" }
            },
            new AuthenticationHeaderValue("Bearer", "rtttdvdtgdtg"),
            null,
            "test",
            CancellationToken.None).ConfigureAwait(false);

        Assert.IsType<Option<GrantedToken>.Result>(option);
    }
}
