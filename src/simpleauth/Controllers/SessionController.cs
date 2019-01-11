namespace SimpleAuth.Server.Controllers
{
    using Extensions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using Shared.Repositories;
    using Shared.Requests;
    using Shared.Serializers;
    using SimpleAuth;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Threading.Tasks;

    public class SessionController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IClientStore _clientRepository;

        public SessionController(
            IAuthenticationService authenticationService,
            IClientStore clientRepository)
        {
            _authenticationService = authenticationService;
            _clientRepository = clientRepository;
        }

        [HttpGet(CoreConstants.EndPoints.CheckSession)]
        public async Task CheckSession()
        {
            await this.DisplayInternalHtml("SimpleAuth.Views.CheckSession.html",
                    (html) => html.Replace("{cookieName}", CoreConstants.SESSION_ID))
                .ConfigureAwait(false);
        }

        [HttpGet(CoreConstants.EndPoints.EndSession)]
        public async Task RevokeSession()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, HostConstants.CookieNames.CookieName).ConfigureAwait(false);
            if (authenticatedUser == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                await this.DisplayInternalHtml("SimpleAuth.Views.UserNotConnected.html").ConfigureAwait(false);
                return;
            }

            var url = CoreConstants.EndPoints.EndSessionCallback;
            if (Request.QueryString.HasValue)
            {
                url = $"{url}{Request.QueryString.Value}";
            }

            await this.DisplayInternalHtml("SimpleAuth.Views.RevokeSession.html", (html) =>
            {
                return html.Replace("{endSessionCallbackUrl}", url);
            }).ConfigureAwait(false);
        }

        [HttpGet(CoreConstants.EndPoints.EndSessionCallback)]
        public async Task RevokeSessionCallback()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, HostConstants.CookieNames.CookieName).ConfigureAwait(false);
            if (authenticatedUser == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                await this.DisplayInternalHtml("SimpleAuth.Views.UserNotConnected.html").ConfigureAwait(false);
                return;
            }

            var query = Request.Query;
            var serializer = new ParamSerializer();
            RevokeSessionRequest request = null;
            if (query != null)
            {
                request = serializer.Deserialize<RevokeSessionRequest>(query.Select(x =>
                    new KeyValuePair<string, string[]>(x.Key, x.Value)));
            }

            Response.Cookies.Delete(CoreConstants.SESSION_ID);
            await _authenticationService.SignOutAsync(HttpContext, HostConstants.CookieNames.CookieName, new AuthenticationProperties()).ConfigureAwait(false);
            if (request != null
                && request.PostLogoutRedirectUri != null
                && !string.IsNullOrWhiteSpace(request.IdTokenHint))
            {
                var handler = new JwtSecurityTokenHandler();
                var tokenValidationParameters = new TokenValidationParameters();
                handler.ValidateToken(request.IdTokenHint, tokenValidationParameters, out var token);
                var jws = (token as JwtSecurityToken)?.Payload;
                //var jws = await _jwtParser.UnSignAsync(request.IdTokenHint).ConfigureAwait(false);
                var claim = jws?.GetClaimValue(StandardClaimNames.Azp);
                if (claim != null)
                {
                    var client = await _clientRepository.GetById(claim).ConfigureAwait(false);
                    if (client?.PostLogoutRedirectUris != null && client.PostLogoutRedirectUris.Contains(request.PostLogoutRedirectUri))
                    {
                        var redirectUrl = request.PostLogoutRedirectUri;
                        if (!string.IsNullOrWhiteSpace(request.State))
                        {
                            redirectUrl = new Uri($"{redirectUrl.AbsoluteUri}?state={request.State}");
                        }

                        Response.Redirect(redirectUrl.AbsoluteUri);
                        return;
                    }
                }
            }

            await this.DisplayInternalHtml("SimpleAuth.Views.RevokeSessionCallback.html").ConfigureAwait(false);
        }
    }
}
