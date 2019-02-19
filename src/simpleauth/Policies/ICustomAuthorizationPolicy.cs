namespace SimpleAuth.Policies
{
    using System.Collections.Generic;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the custom authorization policy interface.
    /// </summary>
    public interface ICustomAuthorizationPolicy
    {
        /// <summary>
        /// Executes the specified valid ticket.
        /// </summary>
        /// <param name="validTicket">The valid ticket.</param>
        /// <param name="authorizationPolicy">The authorization policy.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
        bool Execute(Ticket validTicket, Policy authorizationPolicy, IEnumerable<System.Security.Claims.Claim> claims);
    }
}