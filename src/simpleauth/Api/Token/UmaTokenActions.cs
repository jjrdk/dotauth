using SimpleAuth.Shared.Repositories;

namespace SimpleAuth.Api.Token
{
    using Authenticate;
    using JwtToken;
    using Newtonsoft.Json.Linq;
    using Parameters;
    using Policies;
    using Shared;
    using Shared.Events.Uma;
    using Shared.Models;
    using Shared.Responses;
    using SimpleAuth.Shared.Errors;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class UmaTokenActions
    {
        private readonly ITicketStore _ticketStore;
        private readonly RuntimeSettings _configurationService;
        private readonly AuthorizationPolicyValidator _authorizationPolicyValidator;
        private readonly AuthenticateClient _authenticateClient;
        private readonly JwtGenerator _jwtGenerator;
        private readonly ITokenStore _tokenStore;
        private readonly IEventPublisher _eventPublisher;

        public UmaTokenActions(
            ITicketStore ticketStore,
            RuntimeSettings configurationService,
            IClientStore clientStore,
            IScopeRepository scopeRepository,
            ITokenStore tokenStore,
            IResourceSetRepository resourceSetRepository,
            IPolicyRepository policyRepository,
            IJwksStore jwksStore,
            IEventPublisher eventPublisher)
        {
            _ticketStore = ticketStore;
            _configurationService = configurationService;
            _authorizationPolicyValidator = new AuthorizationPolicyValidator(
                clientStore,
                jwksStore,
                policyRepository,
                resourceSetRepository,
                eventPublisher);
            _authenticateClient = new AuthenticateClient(clientStore, jwksStore);
            _jwtGenerator = new JwtGenerator(clientStore, scopeRepository, jwksStore);
            _tokenStore = tokenStore;
            _eventPublisher = eventPublisher;
        }

        public async Task<GrantedToken> GetTokenByTicketId(
            GetTokenViaTicketIdParameter parameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(parameter.Ticket))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, UmaConstants.RptClaims.Ticket));
            }

            var instruction = authenticationHeaderValue.GetAuthenticateInstruction(parameter, certificate);
            var authResult = await _authenticateClient.Authenticate(instruction, issuerName, cancellationToken)
                .ConfigureAwait(false);
            var client = authResult.Client;
            if (client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            if (client.GrantTypes == null || client.GrantTypes.All(x => x != GrantTypes.UmaTicket))
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidGrant,
                    string.Format(
                        ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        client.ClientId,
                        GrantTypes.UmaTicket));
            }

            var ticket = await _ticketStore.Get(parameter.Ticket, cancellationToken).ConfigureAwait(false);
            if (ticket == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidTicket,
                    string.Format(ErrorDescriptions.TheTicketDoesntExist, parameter.Ticket));
            }

            // 4. Check the ticket.
            if (ticket.ExpirationDateTime < DateTimeOffset.UtcNow)
            {
                throw new SimpleAuthException(ErrorCodes.ExpiredTicket, ErrorDescriptions.TheTicketIsExpired);
            }

            var claimTokenParameter = new ClaimTokenParameter
            {
                Token = parameter.ClaimToken,
                Format = parameter.ClaimTokenFormat
            };

            // 4. Check the authorization.
            var authorizationResult = await _authorizationPolicyValidator
                .IsAuthorized(ticket, client.ClientId, claimTokenParameter, cancellationToken)
                .ConfigureAwait(false);
            if (authorizationResult.Type != AuthorizationPolicyResultEnum.Authorized)
            {
                await _eventPublisher.Publish(
                        new UmaRequestNotAuthorized(Id.Create(), parameter.Ticket, parameter.ClientId, DateTimeOffset.UtcNow))
                    .ConfigureAwait(false);
                throw new SimpleAuthException(
                    ErrorCodes.RequestSubmitted,
                    ErrorDescriptions.TheAuthorizationPolicyIsNotSatisfied);
            }

            // 5. Generate a granted token.
            //var grantedToken = await GenerateToken(client, ticket.Lines, "openid", issuerName).ConfigureAwait(false);
            var grantedToken = await GenerateToken(client, ticket.Lines, "openid", issuerName).ConfigureAwait(false);
            await _tokenStore.AddToken(grantedToken, cancellationToken).ConfigureAwait(false);
            await _ticketStore.Remove(ticket.Id, cancellationToken).ConfigureAwait(false);
            return grantedToken;
        }

        private async Task<GrantedToken> GenerateToken(
            Client client,
            TicketLine[] ticketLines,
            string scope,
            string issuerName)
        {
            var expiresIn = _configurationService.RptLifeTime; // 1. Retrieve the expiration time of the granted token.
            var jwsPayload = await _jwtGenerator.GenerateAccessToken(client, scope.Split(' '), issuerName)
                .ConfigureAwait(false);

            // 2. Construct the JWT token (client).
            jwsPayload.Payload.Add(UmaConstants.RptClaims.Ticket, ticketLines);
            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.WriteToken(jwsPayload);

            return new GrantedToken
            {
                AccessToken = accessToken,
                RefreshToken = Id.Create(),
                ExpiresIn = (int)expiresIn.TotalSeconds,
                TokenType = CoreConstants.StandardTokenTypes.Bearer,
                CreateDateTime = DateTimeOffset.UtcNow,
                Scope = scope,
                ClientId = client.ClientId
            };
        }
    }
}
