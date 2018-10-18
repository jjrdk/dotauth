#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Results;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions;
using SimpleIdentityServer.Manager.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Validators;
using SimpleIdentityServer.Manager.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.ResourceOwners
{
    public interface IResourceOwnerActions
    {
        Task<bool> UpdateResourceOwnerClaims(UpdateResourceOwnerClaimsParameter request);
        Task<bool> UpdateResourceOwnerPassword(UpdateResourceOwnerPasswordParameter request);
        Task<ResourceOwner> GetResourceOwner(string subject);
        Task<ICollection<ResourceOwner>> GetResourceOwners();
        Task<bool> Delete(string subject);
        Task<bool> Add(AddUserParameter parameter);
        Task<SearchResourceOwnerResult> Search(SearchResourceOwnerParameter parameter);
    }

    internal class ResourceOwnerActions : IResourceOwnerActions
    {
        private readonly IGetResourceOwnerAction _getResourceOwnerAction;
        private readonly IGetResourceOwnersAction _getResourceOwnersAction;
        private readonly IUpdateResourceOwnerClaimsAction _updateResourceOwnerClaimsAction;
        private readonly IUpdateResourceOwnerPasswordAction _updateResourceOwnerPasswordAction;
        private readonly IDeleteResourceOwnerAction _deleteResourceOwnerAction;
        private readonly IAddUserOperation _addUserOperation;
        private readonly ISearchResourceOwnersAction _searchResourceOwnersAction;
        private readonly IUpdateResourceOwnerClaimsParameterValidator _updateResourceOwnerClaimsParameterValidator;
        private readonly IUpdateResourceOwnerPasswordParameterValidator _updateResourceOwnerPasswordParameterValidator;
        private readonly IAddUserParameterValidator _addUserParameterValidator;
        private readonly IManagerEventSource _managerEventSource;

        public ResourceOwnerActions(
            IGetResourceOwnerAction getResourceOwnerAction,
            IGetResourceOwnersAction getResourceOwnersAction,
            IUpdateResourceOwnerClaimsAction updateResourceOwnerClaimsAction,
            IUpdateResourceOwnerPasswordAction updateResourceOwnerPasswordAction,
            IDeleteResourceOwnerAction deleteResourceOwnerAction,
            IAddUserOperation addUserOperation,
            ISearchResourceOwnersAction searchResourceOwnersAction,
            IUpdateResourceOwnerClaimsParameterValidator updateResourceOwnerClaimsParameterValidator,
            IUpdateResourceOwnerPasswordParameterValidator updateResourceOwnerPasswordParameterValidator,
            IAddUserParameterValidator addUserParameterValidator,
            IManagerEventSource managerEventSource)
        {
            _getResourceOwnerAction = getResourceOwnerAction;
            _getResourceOwnersAction = getResourceOwnersAction;
            _updateResourceOwnerClaimsAction = updateResourceOwnerClaimsAction;
            _updateResourceOwnerPasswordAction = updateResourceOwnerPasswordAction;
            _deleteResourceOwnerAction = deleteResourceOwnerAction;
            _addUserOperation = addUserOperation;
            _searchResourceOwnersAction = searchResourceOwnersAction;
            _updateResourceOwnerClaimsParameterValidator = updateResourceOwnerClaimsParameterValidator;
            _updateResourceOwnerPasswordParameterValidator = updateResourceOwnerPasswordParameterValidator;
            _addUserParameterValidator = addUserParameterValidator;
            _managerEventSource = managerEventSource;
        }
        
        public Task<ResourceOwner> GetResourceOwner(string subject)
        {
            return _getResourceOwnerAction.Execute(subject);
        }

        public Task<ICollection<ResourceOwner>> GetResourceOwners()
        {
            return _getResourceOwnersAction.Execute();
        }

        public async Task<bool> UpdateResourceOwnerClaims(UpdateResourceOwnerClaimsParameter request)
        {
            _managerEventSource.StartToUpdateResourceOwnerClaims(request.Login);
            _updateResourceOwnerClaimsParameterValidator.Validate(request);
            var result = await _updateResourceOwnerClaimsAction.Execute(request).ConfigureAwait(false);
            _managerEventSource.FinishToUpdateResourceOwnerClaims(request.Login);
            return result;
        }

        public async Task<bool> UpdateResourceOwnerPassword(UpdateResourceOwnerPasswordParameter request)
        {
            _managerEventSource.StartToUpdateResourceOwnerPassword(request.Login);
            _updateResourceOwnerPasswordParameterValidator.Validate(request);
            var result =  await _updateResourceOwnerPasswordAction.Execute(request).ConfigureAwait(false);
            _managerEventSource.FinishToUpdateResourceOwnerPassword(request.Login);
            return result;
        }

        public async Task<bool> Delete(string subject)
        {
            _managerEventSource.StartToRemoveResourceOwner(subject);
            var result = await _deleteResourceOwnerAction.Execute(subject).ConfigureAwait(false);
            _managerEventSource.FinishToRemoveResourceOwner(subject);
            return result;
        }

        public async Task<bool> Add(AddUserParameter parameter)
        {
            _managerEventSource.StartToAddResourceOwner(parameter.Login);
            _addUserParameterValidator.Validate(parameter);
            var result = await _addUserOperation.Execute(parameter, null).ConfigureAwait(false);
            _managerEventSource.FinishToAddResourceOwner(parameter.Login);
            return result;
        }

        public Task<SearchResourceOwnerResult> Search(SearchResourceOwnerParameter parameter)
        {
            return _searchResourceOwnersAction.Execute(parameter);
        }
    }
}
