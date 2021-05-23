namespace SimpleAuth.Shared
{
    using System;
    using System.Linq;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the permission extension methods.
    /// </summary>
    public static class PermissionExtensions
    {
        /// <summary>
        /// Checks whether a <see cref="Permission"/> for a particular resource request.
        /// </summary>
        /// <param name="permission">The <see cref="Permission"/> to check.</param>
        /// <param name="resourceId">The id of the requested resource.</param>
        /// <param name="scopes">The request scopes.</param>
        /// <returns><c>true</c> if the request is valid, otherwise <c>false</c>.</returns>
        public static bool IsValid(this Permission permission, string resourceId, params string[] scopes)
        {
            var now = DateTimeOffset.UtcNow;
            return permission.ResourceSetId == resourceId
                  && permission.NotBefore.ConvertFromUnixTicks() <= now
                  && permission.Expiry.ConvertFromUnixTicks() > now
                  && scopes.All(permission.Scopes.Contains);
        }
    }
}