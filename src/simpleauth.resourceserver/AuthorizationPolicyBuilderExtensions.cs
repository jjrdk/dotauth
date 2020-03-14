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

namespace SimpleAuth.ResourceServer
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using SimpleAuth.ResourceServer.Authentication;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the authorization policy builder extensions.
    /// </summary>
    public static class AuthorizationPolicyBuilderExtensions
    {
        /// <summary>
        /// Adds a policy rule that the token must have a valid <see cref="TicketLine"/> claim.
        /// </summary>
        /// <param name="builder">The <see cref="AuthorizationPolicyBuilder"/> to configure.</param>
        /// <returns>The configured <see cref="AuthorizationPolicyBuilder"/>.</returns>
        public static AuthorizationPolicyBuilder RequireUmaTicket(this AuthorizationPolicyBuilder builder)
        {
            return builder.RequireUmaTicket(UmaAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Adds a policy rule that the token must have a valid <see cref="TicketLine"/> claim.
        /// </summary>
        /// <param name="builder">The <see cref="AuthorizationPolicyBuilder"/> to configure.</param>
        /// <param name="authenticationScheme">The name of the authentication scheme.</param>
        /// <returns>The configured <see cref="AuthorizationPolicyBuilder"/>.</returns>
        public static AuthorizationPolicyBuilder RequireUmaTicket(this AuthorizationPolicyBuilder builder, string authenticationScheme)
        {
            return builder.AddAuthenticationSchemes(authenticationScheme)
                .RequireAuthenticatedUser()
                .RequireAssertion(p => (p.User.Identity as ClaimsIdentity).TryGetUmaTickets(out _));
        }
    }
}