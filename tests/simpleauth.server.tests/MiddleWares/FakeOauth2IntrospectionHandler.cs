namespace SimpleAuth.Server.Tests.MiddleWares
{
    using Client;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Shared;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.Net.Http.Headers;

    public class FakeOauth2IntrospectionHandler : AuthenticationHandler<FakeOAuth2IntrospectionOptions>
    {
        public FakeOauth2IntrospectionHandler(
            IOptionsMonitor<FakeOAuth2IntrospectionOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authorization = Request.Headers[HeaderNames.Authorization];
            if (string.IsNullOrWhiteSpace(authorization))
            {
                return AuthenticateResult.NoResult();
            }

            if (!AuthenticationHeaderValue.TryParse(authorization, out var token))
            {
                return AuthenticateResult.NoResult();
            }

            try
            {
                var introspectionClient = await IntrospectClient.Create(
                        TokenCredentials.FromClientCredentials(Options.ClientId, Options.ClientSecret),
                        Options.Client,
                        new Uri(Options.WellKnownConfigurationUrl))
                    .ConfigureAwait(false);
                var introspectionResult = await introspectionClient.Introspect(
                        IntrospectionRequest.Create(token.Parameter, TokenTypes.AccessToken))
                    .ConfigureAwait(false);
                if (introspectionResult.ContainsError || !introspectionResult.Content.Active)
                {
                    return AuthenticateResult.NoResult();
                }

                var claims = new List<Claim>
                {
                    new Claim(StandardClaimNames.ExpirationTime, introspectionResult.Content.Expiration.ToString()),
                    new Claim(StandardClaimNames.Iat, introspectionResult.Content.IssuedAt.ToString())
                };

                if (!string.IsNullOrWhiteSpace(introspectionResult.Content.Subject))
                {
                    claims.Add(
                        new Claim(
                            OpenIdClaimTypes.Subject,
                            introspectionResult.Content.Subject));
                }

                if (!string.IsNullOrWhiteSpace(introspectionResult.Content.ClientId))
                {
                    claims.Add(new Claim(StandardClaimNames.ClientId, introspectionResult.Content.ClientId));
                }

                if (!string.IsNullOrWhiteSpace(introspectionResult.Content.Issuer))
                {
                    claims.Add(new Claim(StandardClaimNames.Issuer, introspectionResult.Content.Issuer));
                }

                if (introspectionResult.Content.Scope != null)
                {
                    claims.AddRange(introspectionResult.Content.Scope.Select(scope => new Claim(StandardClaimNames.Scopes, scope)));
                }

                var claimsIdentity = new ClaimsIdentity(claims, FakeOAuth2IntrospectionOptions.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                var authenticationTicket = new AuthenticationTicket(
                    claimsPrincipal,
                    new AuthenticationProperties(),
                    FakeOAuth2IntrospectionOptions.AuthenticationScheme);
                return AuthenticateResult.Success(authenticationTicket);
            }
            catch (Exception)
            {
                return AuthenticateResult.NoResult();
            }
        }
    }
}
