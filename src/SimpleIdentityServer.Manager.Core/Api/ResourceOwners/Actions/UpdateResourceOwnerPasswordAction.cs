using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions
{
    public interface IUpdateResourceOwnerPasswordAction
    {
        Task<bool> Execute(UpdateResourceOwnerPasswordParameter request);
    }

    internal sealed class UpdateResourceOwnerPasswordAction : IUpdateResourceOwnerPasswordAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        public UpdateResourceOwnerPasswordAction(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        public async Task<bool> Execute(UpdateResourceOwnerPasswordParameter request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var resourceOwner = await _resourceOwnerRepository.GetAsync(request.Login).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidParameterCode, string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, request.Login));
            }

            resourceOwner.Password = PasswordHelper.ComputeHash(request.Password);
            var result = await _resourceOwnerRepository.UpdateAsync(resourceOwner).ConfigureAwait(false);
            if (!result)
            {
                throw new IdentityServerManagerException(ErrorCodes.InternalErrorCode, ErrorDescriptions.ThePasswordCannotBeUpdated);
            }

            return true;
        }
    }
}
