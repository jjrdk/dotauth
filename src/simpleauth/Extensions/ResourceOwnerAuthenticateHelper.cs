namespace SimpleAuth.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Services;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    internal static class ResourceOwnerAuthenticateHelper
    {
        public static Task<ResourceOwner?> Authenticate(
            this IAuthenticateResourceOwnerService[] services,
            string login,
            string password,
            CancellationToken cancellationToken,
            params string[] exceptedAmrValues)
        {
            var currentAmrs = services.Select(s => s.Amr).ToArray();
            var amr = currentAmrs.GetAmr(exceptedAmrValues);
            if (amr is not Option<string>.Result result)
            {
                return Task.FromResult<ResourceOwner?>(null);
            }
            var service = services.Single(s => s.Amr == result.Item);
            return service.AuthenticateResourceOwner(login, password, cancellationToken);
        }

        public static IEnumerable<string> GetAmrs(this IEnumerable<IAuthenticateResourceOwnerService> services)
        {
            return services.Select(s => s.Amr);
        }
    }
}
