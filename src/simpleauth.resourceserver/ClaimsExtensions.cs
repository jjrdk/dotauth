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
    using System.Linq;
    using System.Security.Claims;
    using Newtonsoft.Json;
    using SimpleAuth.Shared.Models;

    public static class ClaimsExtensions
    {
        public static bool TryGetUmaTickets(this ClaimsIdentity identity, out TicketLine[] tickets)
        {
            tickets = null;
            if (identity == null)
            {
                return false;
            }

            try
            {
                tickets = identity?.Claims?.Where(c => c.Type == "ticket")
                    .Select(c => JsonConvert.DeserializeObject<TicketLine>(c.Value))
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