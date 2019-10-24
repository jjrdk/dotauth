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

namespace SimpleAuth.UserInfoIntrospection
{
    using Client;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.Net.Http.Headers;

    internal class UserInfoIntrospectionHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly UserInfoClient _userInfoClient;

        public UserInfoIntrospectionHandler(
            UserInfoClient userInfoClient,
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _userInfoClient = userInfoClient;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string authorization = Request.Headers[HeaderNames.Authorization];
            if (string.IsNullOrWhiteSpace(authorization)
                || !AuthenticationHeaderValue.TryParse(authorization, out var header))
            {
                return AuthenticateResult.NoResult();
            }

            try
            {
                var introspectionResult = await _userInfoClient.Get(header.Parameter).ConfigureAwait(false);
                if (introspectionResult == null || introspectionResult.HasError)
                {
                    return AuthenticateResult.NoResult();
                }

                var claims = introspectionResult.Content.Claims.ToList();

                var claimsIdentity = new ClaimsIdentity(claims, UserIntrospectionDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                var authenticationTicket = new AuthenticationTicket(
                    claimsPrincipal,
                    new AuthenticationProperties(),
                    UserIntrospectionDefaults.AuthenticationScheme);
                return AuthenticateResult.Success(authenticationTicket);
            }
            catch (Exception)
            {
                return AuthenticateResult.NoResult();
            }
        }
    }
}
