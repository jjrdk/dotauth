// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Shared
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    /// Defines the ClaimPrincipal extensions.
    /// </summary>
    public static class ClaimPrincipalExtensions
    {
        /// <summary>
        /// Returns if the user is authenticated
        /// </summary>
        /// <param name="principal">The user principal</param>
        /// <returns>The user is authenticated</returns>
        public static bool IsAuthenticated(this ClaimsPrincipal principal)
        {
            return principal?.Identity?.IsAuthenticated == true;
        }

        /// <summary>
        /// Returns the subject from an authenticated user
        /// Otherwise returns null.
        /// </summary>
        /// <param name="principal">The user principal</param>
        /// <returns>User's subject</returns>
        public static string GetSubject(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst(OpenIdClaimTypes.Subject)?.Value;
        }

        /// <summary>
        /// Gets the client application id claim value.
        /// </summary>
        /// <param name="principal">The user principal.</param>
        /// <returns>the user's client.</returns>
        public static string GetClientId(this ClaimsPrincipal principal)
        {
            if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
            {
                return string.Empty;
            }

            var claim = principal.Claims.FirstOrDefault(c => c.Type == StandardClaimNames.Azp);
            return claim == null ? string.Empty : claim.Value;
        }

        /// <summary>
        /// Gets the name of the authenticated user.
        /// </summary>
        /// <param name="principal">The user principal.</param>
        /// <returns>The user's name.</returns>
        public static string GetName(this ClaimsPrincipal principal)
        {
            return GetClaimValue(principal, OpenIdClaimTypes.Name)
                   ?? GetClaimValue(principal, StandardClaimNames.Subject)
                   ?? GetClaimValue(principal, ClaimTypes.Name)
                   ?? GetClaimValue(principal, ClaimTypes.NameIdentifier);
        }

        private static string GetClaimValue(ClaimsPrincipal principal, string claimName)
        {
            var claim = principal?.FindFirst(claimName);

            return claim?.Value;
        }
    }
}
