using SimpleIdentityServer.Uma.Core.Parameters;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Core.Api.Token
{
    using Common;
    using Errors;
    using Exceptions;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Policies;
    using SimpleIdentityServer.Core.Authenticate;
    using SimpleIdentityServer.Core.Helpers;
    using SimpleIdentityServer.Core.JwtToken;
    using Stores;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Logging;
    using Shared.Models;
    using SimpleIdentityServer.Core.Jwt;
    using AuthenticationHeaderValueExtensions = SimpleIdentityServer.Core.AuthenticationHeaderValueExtensions;

    internal sealed class UmaTokenActions : IUmaTokenActions
    {
        private readonly ITicketStore _ticketStore;
        private readonly UmaConfigurationOptions _configurationService;
        private readonly IUmaServerEventSource _umaServerEventSource;
        private readonly IAuthorizationPolicyValidator _authorizationPolicyValidator;
        private readonly IAuthenticateClient _authenticateClient;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IClientHelper _clientHelper;
        private readonly ITokenStore _tokenStore;

        public UmaTokenActions(ITicketStore ticketStore,
            UmaConfigurationOptions configurationService,
            IUmaServerEventSource umaServerEventSource,
            IAuthorizationPolicyValidator authorizationPolicyValidator,
            IAuthenticateClient authenticateClient,
            IJwtGenerator jwtGenerator,
            IClientHelper clientHelper,
            ITokenStore tokenStore)
        {
            _ticketStore = ticketStore;
            _configurationService = configurationService;
            _umaServerEventSource = umaServerEventSource;
            _authorizationPolicyValidator = authorizationPolicyValidator;
            _authenticateClient = authenticateClient;
            _jwtGenerator = jwtGenerator;
            _clientHelper = clientHelper;
            _tokenStore = tokenStore;
        }

        public async Task<GrantedToken> GetTokenByTicketId(GetTokenViaTicketIdParameter parameter,
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
                throw new BaseUmaException(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheParameterNeedsToBeSpecified, PostAuthorizationNames.TicketId));
            }

            if (string.IsNullOrWhiteSpace(parameter.Ticket))
            {
                throw new ArgumentNullException(nameof(parameter.Ticket));
            }

            // 2. Try to authenticate the client.
            var instruction = AuthenticationHeaderValueExtensions.GetAuthenticateInstruction(authenticationHeaderValue, parameter, certificate);
            var authResult = await _authenticateClient.AuthenticateAsync(instruction, issuerName).ConfigureAwait(false);
            var client = authResult.Client;
            if (client == null)
            {
                throw new BaseUmaException(ErrorCodes.InvalidClient, authResult.ErrorMessage);
            }

            if (client.GrantTypes == null || !client.GrantTypes.Contains(GrantType.uma_ticket))
            {
                throw new BaseUmaException(ErrorCodes.InvalidGrant,
                    string.Format(ErrorDescriptions.TheClientDoesntSupportTheGrantType,
                        client.ClientId,
                        GrantType.uma_ticket));
            }

            // 3. Retrieve the ticket.
            var json = JsonConvert.SerializeObject(parameter);
            _umaServerEventSource.StartGettingAuthorization(json);
            var ticket = await _ticketStore.GetAsync(parameter.Ticket).ConfigureAwait(false);
            if (ticket == null)
            {
                throw new BaseUmaException(ErrorCodes.InvalidTicket,
                    string.Format(ErrorDescriptions.TheTicketDoesntExist, parameter.Ticket));
            }

            // 4. Check the ticket.
            if (ticket.ExpirationDateTime < DateTime.UtcNow)
            {
                throw new BaseUmaException(ErrorCodes.ExpiredTicket, ErrorDescriptions.TheTicketIsExpired);
            }

            _umaServerEventSource.CheckAuthorizationPolicy(json);
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
                _umaServerEventSource.RequestIsNotAuthorized(json);
                throw new BaseUmaException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationPolicyIsNotSatisfied);
            }

            // 5. Generate a granted token.
            var grantedToken =
                await GenerateTokenAsync(client, ticket.Lines, "openid", issuerName).ConfigureAwait(false);
            await _tokenStore.AddToken(grantedToken).ConfigureAwait(false);
            await _ticketStore.RemoveAsync(ticket.Id).ConfigureAwait(false);
            return grantedToken;
        }

        public async Task<GrantedToken> GenerateTokenAsync(Client client,
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

            var expiresIn = _configurationService.RptLifeTime;// 1. Retrieve the expiration time of the granted token.
            var jwsPayload = await _jwtGenerator.GenerateAccessToken(client, scope.Split(' '), issuerName, null)
                .ConfigureAwait(false); // 2. Construct the JWT token (client).
            var jArr = new JArray();
            foreach (var ticketLine in ticketLines)
            {
                var jObj = new JObject
                {
                    {Constants.RptClaims.ResourceSetId, ticketLine.ResourceSetId},
                    {Constants.RptClaims.Scopes, string.Join(" ", ticketLine.Scopes)}
                };
                jArr.Add(jObj);
            }

            jwsPayload.Add(Constants.RptClaims.Ticket, jArr);
            var accessToken = await _clientHelper.GenerateIdTokenAsync(client, jwsPayload).ConfigureAwait(false);
            var refreshTokenId = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()); // 3. Construct the refresh token.
            return new GrantedToken
            {
                AccessToken = accessToken,
                RefreshToken = Convert.ToBase64String(refreshTokenId),
                ExpiresIn = (int)expiresIn.TotalSeconds,
                TokenType = SimpleIdentityServer.Core.CoreConstants.StandardTokenTypes.Bearer,
                CreateDateTime = DateTime.UtcNow,
                Scope = scope,
                ClientId = client.ClientId
            };
        }
    }
}
