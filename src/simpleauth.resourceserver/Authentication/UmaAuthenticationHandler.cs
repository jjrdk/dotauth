// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.ResourceServer.Authentication
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.DTOs;

    public class UmaAuthenticationHandler : AuthenticationHandler<UmaAuthenticationOptions>
    {
        private readonly IUmaPermissionClient _permissionClient;
        private readonly JwtSecurityTokenHandler _securityTokenHandler;

        public UmaAuthenticationHandler(
            IOptionsMonitor<UmaAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUmaPermissionClient permissionClient)
            : base(options, logger, encoder, clock)
        {
            _permissionClient = permissionClient;
            _securityTokenHandler = new JwtSecurityTokenHandler();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Context.User?.Identity?.IsAuthenticated == true)
            {
                return AuthenticateResult.Success(new AuthenticationTicket(Context.User, Context.User.Identity.AuthenticationType));
            }

            if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
            {
                //Authorization header not in request
                return AuthenticateResult.NoResult();
            }

            if (!AuthenticationHeaderValue.TryParse(
                Request.Headers[HeaderNames.Authorization],
                out var headerValue))
            {
                //Invalid Authorization header
                return AuthenticateResult.NoResult();
            }

            if (!"Bearer".Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                //Not Bearer authentication header
                return AuthenticateResult.NoResult();
            }

            try
            {
                var parameters = Options.TokenValidationParameters.Clone();
                var jsonWebKeySet = await Options.TokenCache.GetJwks().ConfigureAwait(false);
                parameters.IssuerSigningKeys = jsonWebKeySet.GetSigningKeys();
                var principal = _securityTokenHandler.ValidateToken(
                    headerValue.Parameter,
                    parameters,
                    out _);

                if ((principal.Identity as ClaimsIdentity).TryGetUmaTickets(out _))
                {
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);
                    return AuthenticateResult.Success(ticket);
                }

                return AuthenticateResult.NoResult();
            }
            catch (Exception exception)
            {
                Logger.LogError(exception, "Failed to validate token");
                return AuthenticateResult.NoResult();
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (Options.UmaResourcePaths == null || !Options.UmaResourcePaths.Any(r => r.IsMatch(Request.Path)))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                Response.Headers[HeaderNames.WWWAuthenticate] = "Bearer";
                //await base.HandleChallengeAsync(properties);
                return;
            }

            var tokenResponse = await Options.TokenCache.GetToken("uma_protection");
            var ticket = await _permissionClient.RequestPermission(
                new PermissionRequest
                {
                    ResourceSetId = Request.Path.Value.Replace("/data/", string.Empty), Scopes = new[] {"api1"}
                },
                tokenResponse.AccessToken);

            if (ticket.ContainsError)
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                Response.Headers[HeaderNames.Warning] = "199 - \"UMA Authorization Server Unreachable\"";
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                Response.Headers[HeaderNames.WWWAuthenticate] =
                    $"UMA realm=\"{Options.Realm}\", as_uri=\"{Options.Authority}\", ticket=\"{ticket.Content.TicketId}\"";
            }
        }
    }
}
