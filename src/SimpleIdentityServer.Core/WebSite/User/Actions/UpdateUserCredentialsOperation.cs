﻿using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    internal sealed class UpdateUserCredentialsOperation : IUpdateUserCredentialsOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public UpdateUserCredentialsOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<bool> Execute(string subject, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentNullException(nameof(newPassword));
            }

            var resourceOwner = await _resourceOwnerRepository.Get(subject).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheRoDoesntExist);
            }

            resourceOwner.Password = newPassword.ToSha256Hash();
            return await _resourceOwnerRepository.UpdateAsync(resourceOwner).ConfigureAwait(false);
        }
    }
}
