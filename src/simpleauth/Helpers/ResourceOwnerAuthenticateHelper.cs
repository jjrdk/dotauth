namespace SimpleAuth.Helpers
{
    using Services;
    using Shared.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class ResourceOwnerAuthenticateHelper : IResourceOwnerAuthenticateHelper
    {
        private readonly IEnumerable<IAuthenticateResourceOwnerService> _services;

        public ResourceOwnerAuthenticateHelper(IEnumerable<IAuthenticateResourceOwnerService> services)
        {
            _services = services;
        }

        public Task<ResourceOwner> Authenticate(string login, string password, IEnumerable<string> exceptedAmrValues = null)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            var currentAmrs = _services.Select(s => s.Amr);
            var amr = currentAmrs.GetAmr(exceptedAmrValues);
            var service = _services.FirstOrDefault(s => s.Amr == amr);
            return service.AuthenticateResourceOwnerAsync(login, password);
        }

        public IEnumerable<string> GetAmrs()
        {
            return _services.Select(s => s.Amr);
        }
    }
}
