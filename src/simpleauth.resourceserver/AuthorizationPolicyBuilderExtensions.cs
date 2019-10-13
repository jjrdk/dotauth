namespace SimpleAuth.ResourceServer
{
    using System.Linq;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Newtonsoft.Json;
    using SimpleAuth.Shared.Models;

    public static class AuthorizationPolicyBuilderExtensions
    {
        public static AuthorizationPolicyBuilder RequireUmaTicket(this AuthorizationPolicyBuilder builder)
        {
            return builder.RequireAuthenticatedUser()
                .RequireAssertion(p => (p.User.Identity as ClaimsIdentity).TryGetUmaTickets(out _));
        }
    }

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