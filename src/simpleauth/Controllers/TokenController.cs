namespace SimpleAuth.Controllers
{
    using Api.Token;
    using Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Shared;
    using Shared.Models;
    using Shared.Responses;
    using SimpleAuth.Common;
    using SimpleAuth.Repositories;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.Errors;

    /// <summary>
    /// Defines the token controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(UmaConstants.RouteValues.Token)]
    public class TokenController : Controller
    {
        private readonly TokenActions _tokenActions;
        private readonly UmaTokenActions _umaTokenActions;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenController"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="authorizationCodeStore">The authorization code store.</param>
        /// <param name="clientStore">The client store.</param>
        /// <param name="scopeRepository">The scope repository.</param>
        /// <param name="authenticateResourceOwnerServices">The authenticate resource owner services.</param>
        /// <param name="tokenStore">The token store.</param>
        /// <param name="ticketStore">The ticket store.</param>
        /// <param name="resourceSetRepository">The resource set repository.</param>
        /// <param name="eventPublisher">The event publisher.</param>
        public TokenController(
            RuntimeSettings settings,
            IAuthorizationCodeStore authorizationCodeStore,
            IClientStore clientStore,
            IScopeRepository scopeRepository,
            IEnumerable<IAuthenticateResourceOwnerService> authenticateResourceOwnerServices,
            ITokenStore tokenStore,
            ITicketStore ticketStore,
            IResourceSetRepository resourceSetRepository,
            IEventPublisher eventPublisher)
        {
            _tokenActions = new TokenActions(
                settings,
                authorizationCodeStore,
                clientStore,
                scopeRepository,
                authenticateResourceOwnerServices,
                eventPublisher,
                tokenStore);
            _umaTokenActions = new UmaTokenActions(
                ticketStore,
                settings,
                clientStore,
                scopeRepository,
                tokenStore,
                resourceSetRepository,
                eventPublisher);
        }

        /// <summary>
        /// Handles the token request.
        /// </summary>
        /// <param name="tokenRequest">The token request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [HttpPost]
        public async Task<IActionResult> PostToken(
            [FromForm] TokenRequest tokenRequest,
            CancellationToken cancellationToken)
        {
            var certificate = GetCertificate();
            if (tokenRequest.grant_type == null)
            {
                return BadRequest(
                    new ErrorResponse
                    {
                        Error = ErrorCodes.InvalidRequestCode,
                        ErrorDescription = string.Format(
                            ErrorDescriptions.MissingParameter,
                            RequestTokenNames.GrantType)
                    });
            }

            GrantedToken result = null;
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                var authorizationHeaderValue = authorizationHeader[0];
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Length == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }

            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            switch (tokenRequest.grant_type)
            {
                case GrantTypes.Password:
                    result = await GetClientCredentialsGrantedToken(
                            tokenRequest,
                            authenticationHeaderValue,
                            certificate,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false);
                    break;
                case GrantTypes.AuthorizationCode:
                    var authCodeParameter = tokenRequest.ToAuthorizationCodeGrantTypeParameter();
                    result = await _tokenActions.GetTokenByAuthorizationCodeGrantType(
                            authCodeParameter,
                            authenticationHeaderValue,
                            certificate,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false);
                    break;
                case GrantTypes.RefreshToken:
                    var refreshTokenParameter = tokenRequest.ToRefreshTokenGrantTypeParameter();
                    result = await _tokenActions.GetTokenByRefreshTokenGrantType(
                            refreshTokenParameter,
                            authenticationHeaderValue,
                            certificate,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false);
                    break;
                case GrantTypes.ClientCredentials:
                    var clientCredentialsParameter = tokenRequest.ToClientCredentialsGrantTypeParameter();
                    result = await _tokenActions.GetTokenByClientCredentialsGrantType(
                            clientCredentialsParameter,
                            authenticationHeaderValue,
                            certificate,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false);
                    break;
                case GrantTypes.UmaTicket:
                    var tokenIdParameter = tokenRequest.ToTokenIdGrantTypeParameter();
                    result = await _umaTokenActions.GetTokenByTicketId(
                            tokenIdParameter,
                            authenticationHeaderValue,
                            certificate,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false);
                    break;
                case GrantTypes.ValidateBearer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new OkObjectResult(result.ToDto());
        }

        private Task<GrantedToken> GetClientCredentialsGrantedToken(
            TokenRequest tokenRequest,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName,
            CancellationToken cancellationToken)
        {
            var resourceOwnerParameter = tokenRequest.ToResourceOwnerGrantTypeParameter();
            return _tokenActions.GetTokenByResourceOwnerCredentialsGrantType(
                resourceOwnerParameter,
                authenticationHeaderValue,
                certificate,
                issuerName,
                cancellationToken);
        }

        /// <summary>
        /// Handles the token revocation.
        /// </summary>
        /// <param name="revocationRequest">The revocation request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost("revoke")]
        public async Task<IActionResult> PostRevoke(
            [FromForm] RevocationRequest revocationRequest,
            CancellationToken cancellationToken)
        {
            // 1. Fetch the authorization header
            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                var authorizationHeaderValue = authorizationHeader.First();
                var splittedAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splittedAuthorizationHeaderValue.Length == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splittedAuthorizationHeaderValue[0],
                        splittedAuthorizationHeaderValue[1]);
                }
            }

            // 2. Revoke the token
            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var result = await _tokenActions.RevokeToken(
                    revocationRequest.ToParameter(),
                    authenticationHeaderValue,
                    GetCertificate(),
                    issuerName,
                    cancellationToken)
                .ConfigureAwait(false);
            return result ? new OkResult() : StatusCode((int) HttpStatusCode.BadRequest);
        }

        private X509Certificate2 GetCertificate()
        {
            const string headerName = "X-ARR-ClientCert";
            var header = Request.Headers.FirstOrDefault(h => h.Key == headerName);
            if (header.Equals(default(KeyValuePair<string, StringValues>)))
            {
                return null;
            }

            try
            {
                var encoded = Convert.FromBase64String(header.Value);
                return new X509Certificate2(encoded);
            }
            catch
            {
                return null;
            }
        }
    }
}
