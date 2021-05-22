namespace SimpleAuth.Shared
{
    using System;
    using System.Linq;
    using SimpleAuth.Shared.Models;

    public static class PermissionExtensions
    {
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