namespace DotAuth.Api.Token;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Authenticate;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.JwtToken;
using DotAuth.Parameters;
using DotAuth.Policies;
using DotAuth.Properties;
using DotAuth.Shared;
using DotAuth.Shared.Errors;
using DotAuth.Shared.Events.Uma;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Responses;
using Microsoft.Extensions.Logging;

internal sealed class UmaTokenActions
{
    private readonly ITicketStore _ticketStore;
    private readonly RuntimeSettings _configurationService;
    private readonly AuthorizationPolicyValidator _authorizationPolicyValidator;
    private readonly AuthenticateClient _authenticateClient;
    private readonly JwtGenerator _jwtGenerator;
    private readonly ITokenStore _tokenStore;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger _logger;

    public UmaTokenActions(
        ITicketStore ticketStore,
        RuntimeSettings configurationService,
        IClientStore clientStore,
        IScopeStore scopeRepository,
        ITokenStore tokenStore,
        IResourceSetRepository resourceSetRepository,
        IJwksStore jwksStore,
        IEventPublisher eventPublisher,
        ILogger logger)
    {
        _ticketStore = ticketStore;
        _configurationService = configurationService;
        _authorizationPolicyValidator = new AuthorizationPolicyValidator(
            jwksStore,
            resourceSetRepository,
            eventPublisher);
        _authenticateClient = new AuthenticateClient(clientStore, jwksStore);
        _jwtGenerator = new JwtGenerator(clientStore, scopeRepository, jwksStore, logger);
        _tokenStore = tokenStore;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Option<GrantedToken>> GetTokenByTicketId(
        GetTokenViaTicketIdParameter parameter,
        AuthenticationHeaderValue? authenticationHeaderValue,
        X509Certificate2? certificate,
        string issuerName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(parameter.Ticket))
        {
            _logger.LogError("Ticket is null or empty");
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidRequest,
                Detail = string.Format(Strings.MissingParameter, UmaConstants.RptClaims.Ticket)
            };
        }

        var instruction = authenticationHeaderValue.GetAuthenticateInstruction(parameter, certificate);
        var authResult = await _authenticateClient.Authenticate(instruction, issuerName, cancellationToken)
            .ConfigureAwait(false);
        var client = authResult.Client;
        if (client == null)
        {
            _logger.LogError("Client not found.");
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidClient,
                Detail = authResult.ErrorMessage!
            };
        }

        if (client.GrantTypes.All(x => x != GrantTypes.UmaTicket))
        {
            _logger.LogError("UMA Grant type not supported");
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidGrant,
                Detail = string.Format(
                    Strings.TheClientDoesntSupportTheGrantType,
                    client.ClientId,
                    GrantTypes.UmaTicket)
            };
        }

        var ticket = await _ticketStore.Get(parameter.Ticket, cancellationToken).ConfigureAwait(false);
        if (ticket == null)
        {
            _logger.LogError($"Ticket {parameter.Ticket} not found");
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.InvalidGrant,
                Detail = string.Format(Strings.TheTicketDoesntExist, parameter.Ticket)
            };
        }

        // 4. Check the ticket.
        if (ticket.Expires < DateTimeOffset.UtcNow)
        {
            _logger.LogError("Ticket expired");
            return new ErrorDetails
            {
                Status = HttpStatusCode.BadRequest,
                Title = ErrorCodes.ExpiredTicket,
                Detail = Strings.TheTicketIsExpired
            };
        }

        // 4. Check the authorization.
        var authorizationResult = await _authorizationPolicyValidator
            .IsAuthorized(ticket, client, parameter.ClaimToken, cancellationToken)
            .ConfigureAwait(false);

        if (authorizationResult.Result == AuthorizationPolicyResultKind.Authorized)
        {
            var claimToken = parameter.ClaimToken.Token;
            var grantedToken = await GenerateToken(
                    client,
                    ticket.Lines,
                    "openid",
                    issuerName,
                    claimToken)
                .ConfigureAwait(false);
            if (await _tokenStore.AddToken(grantedToken, cancellationToken).ConfigureAwait(false))
            {
                await _ticketStore.Remove(ticket.Id, cancellationToken).ConfigureAwait(false);
                return grantedToken;
            }

            await _eventPublisher.Publish(
                new RptIssued(
                    Id.Create(),
                    ticket.Id,
                    client.ClientId,
                    ticket.ResourceOwner,
                    authorizationResult.Principal.Claims.Select(claim => new ClaimData { Type = claim.Type, Value = claim.Value }),
                    DateTimeOffset.UtcNow)).ConfigureAwait(false);
            return new ErrorDetails
            {
                Status = HttpStatusCode.InternalServerError,
                Title = ErrorCodes.InternalError,
                Detail = Strings.InternalError
            };
        }

        if (authorizationResult.Result == AuthorizationPolicyResultKind.RequestSubmitted)
        {
            await _eventPublisher.Publish(
                    new UmaRequestSubmitted(
                        Id.Create(),
                        parameter.Ticket,
                        parameter.ClientId ?? string.Empty,
                        authorizationResult.Principal.Claims.Select(claim => new ClaimData { Type = claim.Type, Value = claim.Value }),
                        DateTimeOffset.UtcNow))
                .ConfigureAwait(false);
            return new ErrorDetails
            {
                Status = HttpStatusCode.Forbidden,
                Title = ErrorCodes.RequestSubmitted,
                Detail = Strings.PermissionRequested
            };
        }

        await _eventPublisher.Publish(
                new UmaRequestNotAuthorized(
                    Id.Create(),
                    parameter.Ticket,
                    parameter.ClientId ?? string.Empty,
                    authorizationResult.Principal.Claims.Select(claim => new ClaimData { Type = claim.Type, Value = claim.Value }),
                    DateTimeOffset.UtcNow))
            .ConfigureAwait(false);
        return new ErrorDetails
        {
            Status = HttpStatusCode.BadRequest,
            Title = ErrorCodes.RequestDenied,
            Detail = Strings.TheAuthorizationPolicyIsNotSatisfied
        };
    }

    private async Task<GrantedToken> GenerateToken(
        Client client,
        TicketLine[] ticketLines,
        string scope,
        string issuerName,
        string? idToken)
    {
        // 1. Retrieve the expiration time of the granted token.
        var expiresIn = _configurationService.RptLifeTime;
        var scopes = scope.Split(' ');
        var jwsPayload = await _jwtGenerator.GenerateAccessToken(client, scopes, issuerName)
            .ConfigureAwait(false);

        // 2. Construct the JWT token (client).
        var permissions = ticketLines.Select(x => x.ToPermission(_configurationService.RptLifeTime))
            .ToArray();
        jwsPayload.Payload.Add(UmaConstants.RptClaims.Permissions, permissions);
        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.WriteToken(jwsPayload);

        return new GrantedToken
        {
            Id = Id.Create(),
            AccessToken = accessToken,
            RefreshToken = scopes.Contains(CoreConstants.Offline) ? Id.Create() : null,
            ExpiresIn = (int)expiresIn.TotalSeconds,
            TokenType = CoreConstants.StandardTokenTypes.Bearer,
            CreateDateTime = DateTimeOffset.UtcNow,
            Scope = scope,
            ClientId = client.ClientId,
            IdToken = idToken
        };
    }
}