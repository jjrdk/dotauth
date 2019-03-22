namespace SimpleAuth.Controllers
{
    using Api.Token;
    using Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Shared;
    using Shared.Models;
    using SimpleAuth.Common;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

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
        /// <param name="jwksStore"></param>
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
            IJwksStore jwksStore,
            IResourceSetRepository resourceSetRepository,
            IEventPublisher eventPublisher)
        {
            _tokenActions = new TokenActions(
                settings,
                authorizationCodeStore,
                clientStore,
                scopeRepository,
                jwksStore,
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
                jwksStore,
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
                    new ErrorDetails
                    {
                        Status = HttpStatusCode.BadRequest,
                        Title = ErrorCodes.InvalidRequestCode,
                        Detail = string.Format(
                            ErrorDescriptions.MissingParameter,
                            RequestTokenNames.GrantType)
                    });
            }

            AuthenticationHeaderValue authenticationHeaderValue = null;
            if (Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
            {
                var authorizationHeaderValue = authorizationHeader[0];
                var splitAuthorizationHeaderValue = authorizationHeaderValue.Split(' ');
                if (splitAuthorizationHeaderValue.Length == 2)
                {
                    authenticationHeaderValue = new AuthenticationHeaderValue(
                        splitAuthorizationHeaderValue[0],
                        splitAuthorizationHeaderValue[1]);
                }
            }

            var issuerName = Request.GetAbsoluteUriWithVirtualPath();
            var result = await GetGrantedToken(tokenRequest, cancellationToken, authenticationHeaderValue, certificate, issuerName).ConfigureAwait(false);

            return new OkObjectResult(result.ToDto());
        }

        private async Task<GrantedToken> GetGrantedToken(
            TokenRequest tokenRequest,
            CancellationToken cancellationToken,
            AuthenticationHeaderValue authenticationHeaderValue,
            X509Certificate2 certificate,
            string issuerName)
        {
            switch (tokenRequest.grant_type)
            {
                case GrantTypes.Password:
                    return await GetClientCredentialsGrantedToken(
                            tokenRequest,
                            authenticationHeaderValue,
                            certificate,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false);
                case GrantTypes.AuthorizationCode:
                    var authCodeParameter = tokenRequest.ToAuthorizationCodeGrantTypeParameter();
                    return await _tokenActions.GetTokenByAuthorizationCodeGrantType(
                            authCodeParameter,
                            authenticationHeaderValue,
                            certificate,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false);
                case GrantTypes.RefreshToken:
                    var refreshTokenParameter = tokenRequest.ToRefreshTokenGrantTypeParameter();
                    return await _tokenActions.GetTokenByRefreshTokenGrantType(
                            refreshTokenParameter,
                            authenticationHeaderValue,
                            certificate,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false);
                case GrantTypes.ClientCredentials:
                    var clientCredentialsParameter = tokenRequest.ToClientCredentialsGrantTypeParameter();
                    return await _tokenActions.GetTokenByClientCredentialsGrantType(
                            clientCredentialsParameter,
                            authenticationHeaderValue,
                            certificate,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false);
                case GrantTypes.UmaTicket:
                    var tokenIdParameter = tokenRequest.ToTokenIdGrantTypeParameter();
                    return await _umaTokenActions.GetTokenByTicketId(
                            tokenIdParameter,
                            authenticationHeaderValue,
                            certificate,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false);
                case GrantTypes.ValidateBearer:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            return result ? new OkResult() : StatusCode((int)HttpStatusCode.BadRequest);
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
