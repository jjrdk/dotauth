using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.JwtToken;
using SimpleIdentityServer.Host.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Host.Controllers.Api
{
    using System;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Serializers;

    public class SessionController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IClientStore _clientRepository;
        private readonly IJwtParser _jwtParser;

        public SessionController(
            IAuthenticationService authenticationService,
            IClientStore clientRepository,
            IJwtParser jwtParser)
        {
            _authenticationService = authenticationService;
            _clientRepository = clientRepository;
            _jwtParser = jwtParser;
        }

        [HttpGet(Core.CoreConstants.EndPoints.CheckSession)]
        public async Task CheckSession()
        {
            await this.DisplayInternalHtml("SimpleIdentityServer.Host.Views.CheckSession.html",
                    (html) => html.Replace("{cookieName}", Core.CoreConstants.SESSION_ID))
                .ConfigureAwait(false);
        }

        [HttpGet(Core.CoreConstants.EndPoints.EndSession)]
        public async Task RevokeSession()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, HostConstants.CookieNames.CookieName).ConfigureAwait(false);
            if (authenticatedUser == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                await this.DisplayInternalHtml("SimpleIdentityServer.Host.Views.UserNotConnected.html").ConfigureAwait(false);
                return;
            }

            var url = Core.CoreConstants.EndPoints.EndSessionCallback;
            if (Request.QueryString.HasValue)
            {
                url = $"{url}{Request.QueryString.Value}";
            }

            await this.DisplayInternalHtml("SimpleIdentityServer.Host.Views.RevokeSession.html", (html) =>
            {
                return html.Replace("{endSessionCallbackUrl}", url);
            }).ConfigureAwait(false);
        }

        [HttpGet(Core.CoreConstants.EndPoints.EndSessionCallback)]
        public async Task RevokeSessionCallback()
        {
            var authenticatedUser = await _authenticationService.GetAuthenticatedUser(this, HostConstants.CookieNames.CookieName).ConfigureAwait(false);
            if (authenticatedUser == null || !authenticatedUser.Identity.IsAuthenticated)
            {
                await this.DisplayInternalHtml("SimpleIdentityServer.Host.Views.UserNotConnected.html").ConfigureAwait(false);
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

            Response.Cookies.Delete(Core.CoreConstants.SESSION_ID);
            await _authenticationService.SignOutAsync(HttpContext, HostConstants.CookieNames.CookieName, new AuthenticationProperties()).ConfigureAwait(false);
            if (request != null
                && request.PostLogoutRedirectUri != null
                && !string.IsNullOrWhiteSpace(request.IdTokenHint))
            {
                var jws = await _jwtParser.UnSignAsync(request.IdTokenHint).ConfigureAwait(false);
                var claim = jws?.GetStringClaim(StandardClaimNames.Azp);
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

            await this.DisplayInternalHtml("SimpleIdentityServer.Host.Views.RevokeSessionCallback.html").ConfigureAwait(false);
        }
    }
}
