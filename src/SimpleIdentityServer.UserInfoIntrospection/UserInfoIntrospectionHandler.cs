namespace SimpleAuth.UserInfoIntrospection
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Client;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;

    public class UserInfoIntrospectionHandler : AuthenticationHandler<UserInfoIntrospectionOptions>
    {
        private readonly IUserInfoClient _userInfoClient;
        private static readonly int StartIndex = "Bearer ".Length;

        public UserInfoIntrospectionHandler(IOptionsMonitor<UserInfoIntrospectionOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IUserInfoClient userInfoClient,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _userInfoClient = userInfoClient;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authorization = Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(authorization))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string token = null;
            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring(StartIndex).Trim();
            }

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            return HandleAuthenticate(Options.WellKnownConfigurationUrl, token);
        }
        
        internal async Task<AuthenticateResult> HandleAuthenticate(string wellKnownConfiguration, string token)
        {
            try
            {
                var introspectionResult = await _userInfoClient
                    .Resolve(wellKnownConfiguration, token)
                    .ConfigureAwait(false);
                if (introspectionResult == null || introspectionResult.ContainsError)
                {
                    return AuthenticateResult.NoResult();
                }

                var claims = new List<Claim>();
                var values = introspectionResult.Content.ToObject<Dictionary<string, object>>();
                foreach (var kvp in values)
                {
                    claims.AddRange(Convert(kvp));
                }

                var claimsIdentity = new ClaimsIdentity(claims, UserInfoIntrospectionOptions.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                var authenticationTicket = new AuthenticationTicket(claimsPrincipal, new AuthenticationProperties(), UserInfoIntrospectionOptions.AuthenticationScheme);
                return AuthenticateResult.Success(authenticationTicket);
            }
            catch (Exception)
            {
                return AuthenticateResult.NoResult();
            }
        }

        private static List<Claim> Convert(KeyValuePair<string, object> kvp)
        {
            if (!(kvp.Value is JArray arr))
            {
                return new List<Claim>
                {
                    new Claim(kvp.Key, kvp.Value.ToString())
                };
            }

            var result = new List<Claim>();
            foreach(var r in arr)
            {
                result.Add(new Claim(kvp.Key, r.ToString()));
}

            return result;
        }
    }
}