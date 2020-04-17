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
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using Newtonsoft.Json;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the claims extensions.
    /// </summary>
    public static class ClaimsExtensions
    {
        /// <summary>
        /// Tries to get the ticket lines from the current user claims.
        /// </summary>
        /// <param name="identity">The user as a <see cref="ClaimsIdentity"/> instance.</param>
        /// <param name="tickets">The found array of <see cref="TicketLine"/>. If none are found, then returns an empty array.
        /// If no user is found then returns <c>null</c>.</param>
        /// <returns><c>true</c> if any tickets are found, otherwise <c>false</c>.</returns>
        public static bool TryGetUmaTickets(this ClaimsIdentity identity, out Permission[] tickets)
        {
            Permission[] t = null;
            var result = identity?.Claims.TryGetUmaTickets(out t);
            tickets = t;
            return result == true;
        }

        /// <summary>
        /// Tries to get the ticket lines from the current user claims.
        /// </summary>
        /// <param name="claims">The user claims.</param>
        /// <param name="tickets">The found array of <see cref="TicketLine"/>. If none are found, then returns an empty array.
        /// If no user is found then returns <c>null</c>.</param>
        /// <returns><c>true</c> if any tickets are found, otherwise <c>false</c>.</returns>
        public static bool TryGetUmaTickets(this IEnumerable<Claim> claims, out Permission[] tickets)
        {
            tickets = null;
            if (claims == null)
            {
                return false;
            }

            try
            {
                tickets = claims.Where(c => c.Type == "permissions")
                    .Select(c => JsonConvert.DeserializeObject<Permission>(c.Value))
                    .ToArray();
                return tickets?.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
