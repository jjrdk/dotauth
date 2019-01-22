using System.Linq;

namespace SimpleAuth.Helpers
{
    using Services;
    using Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal static class ResourceOwnerAuthenticateHelper
    {
        public static Task<ResourceOwner> Authenticate(this IEnumerable<IAuthenticateResourceOwnerService> services, string login, string password, IEnumerable<string> exceptedAmrValues = null)
        {
            var currentAmrs = services.Select(s => s.Amr);
            var amr = currentAmrs.GetAmr(exceptedAmrValues);
            var service = services.FirstOrDefault(s => s.Amr == amr);
            return service.AuthenticateResourceOwnerAsync(login, password);
        }

        public static IEnumerable<string> GetAmrs(this IEnumerable<IAuthenticateResourceOwnerService> services)
        {
            return services.Select(s => s.Amr);
        }
    }
}
