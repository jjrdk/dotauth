using SimpleAuth.Shared.Repositories;

namespace SimpleAuth.Api.Token
{
    using Authenticate;
    using Errors;
    using Exceptions;
    using JwtToken;
    using Newtonsoft.Json.Linq;
    using Parameters;
    using Policies;
    using Repositories;
    using Shared;
    using Shared.Events.Uma;
    using Shared.Models;
    using Shared.Responses;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;

    internal sealed class UmaTokenActions : IUmaTokenActions
    {
        private readonly ITicketStore _ticketStore;
        private readonly UmaConfigurationOptions _configurationService;
        private readonly IAuthorizationPolicyValidator _authorizationPolicyValidator;
        private readonly AuthenticateClient _authenticateClient;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly ITokenStore _tokenStore;
        private readonly IEventPublisher _eventPublisher;

        public UmaTokenActions(
            ITicketStore ticketStore,
            UmaConfigurationOptions configurationService,
            IAuthorizationPolicyValidator authorizationPolicyValidator,
            IClientStore clientStore,
            IJwtGenerator jwtGenerator,
            ITokenStore tokenStore,
            IEventPublisher eventPublisher)
        {
            _ticketStore = ticketStore;
            _configurationService = configurationService;
            _authorizationPolicyValidator = authorizationPolicyValidator;
            _authenticateClient = new AuthenticateClient(clientStore);
            _jwtGenerator = jwtGenerator;
            _tokenStore = tokenStore;
            _eventPublisher = eventPublisher;
        }

        public async Task<GrantedToken> GetTokenByTicketId(
            GetTokenViaTicketIdParameter parameter,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName)
        {
            // 1. Check parameters.
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (string.IsNullOrWhiteSpace(parameter.Ticket))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode,
                    string.Format(
                        ErrorDescriptions.TheParameterNeedsToBeSpecified,
                        PostAuthorizationNames.TicketId));
            }

            if (string.IsNullOrWhiteSpace(parameter.Ticket))
            {
                throw new ArgumentNullException(nameof(parameter.Ticket));
            }

            // 2. Try to authenticate the client.
            var instruction =
                authenticationHeaderValue.GetAuthenticateInstruction(
                    parameter,
                    certificate);
            var authResult = await _authenticateClient.Authenticate(instruction, issuerName).ConfigureAwait(false);
            var client = authResult.Client;
            if (client == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantType.uma_ticket))
            {
                throw new SimpleAuthException(ErrorCodes.InvalidGrant,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType, client.ClientId, GrantType.uma_ticket));
            }

            // 3. Retrieve the ticket.
            var ticket = await _ticketStore.Get(parameter.Ticket).ConfigureAwait(false);
            if (ticket == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidTicket,
                    string.Format(ErrorDescriptions.TheTicketDoesntExist, parameter.Ticket));
            }

            // 4. Check the ticket.
            if (ticket.ExpirationDateTime < DateTime.UtcNow)
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
                .IsAuthorized(ticket, client.ClientId, claimTokenParameter)
                .ConfigureAwait(false);
            if (authorizationResult.Type != AuthorizationPolicyResultEnum.Authorized)
            {
                await _eventPublisher.Publish(new UmaRequestNotAuthorized(
                        Id.Create(),
                        parameter.Ticket,
                        parameter.ClientId,
                        DateTime.UtcNow))
                    .ConfigureAwait(false);
                throw new SimpleAuthException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationPolicyIsNotSatisfied);
            }

            // 5. Generate a granted token.
            var grantedToken = GenerateTokenAsync(client, ticket.Lines, "openid", issuerName);
            await _tokenStore.AddToken(grantedToken).ConfigureAwait(false);
            await _ticketStore.Remove(ticket.Id).ConfigureAwait(false);
            return grantedToken;
        }

        public GrantedToken GenerateTokenAsync(
            Client client,
            IEnumerable<TicketLine> ticketLines,
            string scope,
            string issuerName)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (ticketLines == null)
            {
                throw new ArgumentNullException(nameof(ticketLines));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentNullException(nameof(scope));
            }

            var expiresIn = _configurationService.RptLifeTime; // 1. Retrieve the expiration time of the granted token.
            var jwsPayload = _jwtGenerator.GenerateAccessToken(client, scope.Split(' '), issuerName, null);
            // 2. Construct the JWT token (client).
            var jArr = new JArray();
            foreach (var ticketLine in ticketLines)
            {
                var jObj = new JObject
                {
                    {UmaConstants.RptClaims.ResourceSetId, ticketLine.ResourceSetId},
                    {UmaConstants.RptClaims.Scopes, string.Join(" ", ticketLine.Scopes)}
                };
                jArr.Add(jObj);
            }

            jwsPayload.Payload.Add(UmaConstants.RptClaims.Ticket, jArr);
            var handler = new JwtSecurityTokenHandler();
            var accessToken = handler.WriteToken(jwsPayload);
            var refreshTokenId = Encoding.UTF8.GetBytes(Id.Create());
            // 3. Construct the refresh token.
            return new GrantedToken
            {
                AccessToken = accessToken,
                RefreshToken = Convert.ToBase64String(refreshTokenId),
                ExpiresIn = (int)expiresIn.TotalSeconds,
                TokenType = CoreConstants.StandardTokenTypes.Bearer,
                CreateDateTime = DateTime.UtcNow,
                Scope = scope,
                ClientId = client.ClientId
            };
        }
    }
}
