namespace SimpleAuth.Server.Services
{
    using System;
    using System.Threading.Tasks;
    using Helpers;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth.Services;

    internal sealed class PasswordAuthenticateResourceOwnerService : IAuthenticateResourceOwnerService
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public PasswordAuthenticateResourceOwnerService(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public string Amr => "pwd";

        public Task<ResourceOwner> AuthenticateResourceOwnerAsync(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            return _resourceOwnerRepository.Get(login, password.ToSha256Hash());
        }
    }
}