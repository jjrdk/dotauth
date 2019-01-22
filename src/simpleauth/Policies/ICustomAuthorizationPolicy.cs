namespace SimpleAuth.Policies
{
    using System.Collections.Generic;
    using SimpleAuth.Shared.Models;

    public interface ICustomAuthorizationPolicy
    {
        bool Execute(Ticket validTicket, Policy authorizationPolicy, IEnumerable<System.Security.Claims.Claim> claims);
    }
}