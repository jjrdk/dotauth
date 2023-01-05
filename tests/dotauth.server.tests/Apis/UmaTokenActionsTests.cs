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
using Moq;
using Xunit;

public class UmaTokenActionsTests
{
    private readonly UmaTokenActions _tokenActions;

    public UmaTokenActionsTests()
    {
        var ticketStore = new Mock<ITicketStore>();
        ticketStore.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
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
        ticketStore.Setup(x => x.Remove(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var clientStore = new Mock<IClientStore>();
        clientStore.Setup(x => x.GetById(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Client
                {
                    ClientId = "test_client",
                    GrantTypes = new[] { GrantTypes.UmaTicket },
                    TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.ClientSecretPost,
                    Secrets = new[] { new ClientSecret { Type = ClientSecretTypes.SharedSecret, Value = "secret" } }
                });
        var scopeStore = new Mock<IScopeStore>();
        var tokenStore = new Mock<ITokenStore>();
        tokenStore.Setup(x => x.AddToken(It.IsAny<GrantedToken>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var authorizationPolicyValidator = new Mock<IAuthorizationPolicyValidator>();
        authorizationPolicyValidator
            .Setup(
                x => x.IsAuthorized(
                    It.IsAny<Ticket>(),
                    It.IsAny<Client>(),
                    It.IsAny<ClaimTokenParameter>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new AuthorizationPolicyResult(AuthorizationPolicyResultKind.Authorized, new ClaimsPrincipal()));
        var jwksStore = new Mock<IJwksStore>();
        var eventPublisher = new Mock<IEventPublisher>();
        _tokenActions = new UmaTokenActions(
            ticketStore.Object,
            new RuntimeSettings(),
            clientStore.Object,
            scopeStore.Object,
            tokenStore.Object,
            jwksStore.Object,
            authorizationPolicyValidator.Object,
            eventPublisher.Object,
            NullLogger.Instance);
    }

    [Fact]
    public async Task CanGenerateTokenFromValidTicketRequest()
    {
        var option = await _tokenActions.GetTokenByTicketId(
                new GetTokenViaTicketIdParameter { Ticket = "ticket_id", ClientId = "client", ClientSecret = "secret", ClaimToken = new ClaimTokenParameter { Format = "", Token = "token" } },
                new AuthenticationHeaderValue("Bearer", "rtttdvdtgdtg"),
                null,
                "test",
                CancellationToken.None);

        Assert.IsType<Option<GrantedToken>.Result>(option);
    }
}