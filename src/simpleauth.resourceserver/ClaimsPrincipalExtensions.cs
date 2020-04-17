namespace SimpleAuth.ResourceServer
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using SimpleAuth.Shared;

    /// <summary>
    /// Defines the claims principal extensions.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Checks whether the <see cref="ClaimsPrincipal"/> has access to the requested resource.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> seeking access.</param>
        /// <param name="registration">The <see cref="ResourceRegistration"/> to access.</param>
        /// <param name="scope">The access scope.</param>
        /// <returns></returns>
        public static bool CheckResourceAccess(
            this ClaimsPrincipal principal,
            ResourceRegistration registration,
            string[] scope)
        {
            var now = DateTimeOffset.UtcNow;
            return registration.Owner == principal.GetSubject()
                   || ((principal.Identity as ClaimsIdentity).TryGetUmaTickets(out var permissions)
                       && permissions.Any(
                           l => l.ResourceSetId == registration.ResourceSetId
                                && DateTimeExtensions.ConvertFromUnixTicks(l.NotBefore) <= now
                                && DateTimeExtensions.ConvertFromUnixTicks(l.Expiry) > now
                                && scope.All(l.Scopes.Contains)));
        }
    }
}