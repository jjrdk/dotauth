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

namespace SimpleIdentityServer.Manager.Core.Api.Clients.Actions
{
    using Newtonsoft.Json;
    using SimpleIdentityServer.Core.Common;
    using SimpleIdentityServer.Core.Common.Models;
    using SimpleIdentityServer.Core.Common.Repositories;
    using SimpleIdentityServer.Core.Exceptions;
    using SimpleIdentityServer.Manager.Core.Errors;
    using SimpleIdentityServer.Manager.Core.Exceptions;
    using SimpleIdentityServer.Manager.Core.Parameters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Logging;

    public interface IUpdateClientAction
    {
        Task<bool> Execute(UpdateClientParameter updateClientParameter);
    }

    internal sealed class UpdateClientAction : IUpdateClientAction
    {
        private readonly IClientRepository _clientRepository;
        private readonly IClientStore _clientStore;
        private readonly IGenerateClientFromRegistrationRequest _generateClientFromRegistrationRequest;
        private readonly IScopeRepository _scopeRepository;
        private readonly IManagerEventSource _managerEventSource;

        public UpdateClientAction(
            IClientStore clientStore,
            IClientRepository clientRepository,
            IGenerateClientFromRegistrationRequest generateClientFromRegistrationRequest,
            IScopeRepository scopeRepository,
            IManagerEventSource managerEventSource)
        {
            _clientStore = clientStore;
            _clientRepository = clientRepository;
            _generateClientFromRegistrationRequest = generateClientFromRegistrationRequest;
            _scopeRepository = scopeRepository;
            _managerEventSource = managerEventSource;
        }

        public async Task<bool> Execute(UpdateClientParameter updateClientParameter)
        {
            if (updateClientParameter == null)
            {
                throw new ArgumentNullException(nameof(updateClientParameter));
            }

            if (string.IsNullOrWhiteSpace(updateClientParameter.ClientId))
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidParameterCode,
                    string.Format(ErrorDescriptions.MissingParameter, "client_id"));
            }

            _managerEventSource.StartToUpdateClient(JsonConvert.SerializeObject(updateClientParameter));
            var existedClient = await _clientStore.GetById(updateClientParameter.ClientId).ConfigureAwait(false);
            if (existedClient == null)
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidParameterCode,
                    string.Format(ErrorDescriptions.TheClientDoesntExist, updateClientParameter.ClientId));
            }

            Client client = null;
            try
            {
                client = _generateClientFromRegistrationRequest.Execute(updateClientParameter);
            }
            catch (IdentityServerException ex)
            {
                throw new IdentityServerManagerException(ex.Code, ex.Message);
            }

            client.ClientId = existedClient.ClientId;
            client.AllowedScopes = updateClientParameter.AllowedScopes == null
                ? new List<Scope>()
                : updateClientParameter.AllowedScopes.Select(s => new Scope
                {
                    Name = s
                })
                    .ToList();
            var existingScopes = await _scopeRepository.SearchByNamesAsync(client.AllowedScopes.Select(s => s.Name))
                .ConfigureAwait(false);
            var notSupportedScopes = client.AllowedScopes.Where(s => !existingScopes.Any(sc => sc.Name == s.Name))
                .Select(s => s.Name);
            if (notSupportedScopes.Any())
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidParameterCode,
                    string.Format(ErrorDescriptions.TheScopesDontExist, string.Join(",", notSupportedScopes)));
            }

            var result = await _clientRepository.UpdateAsync(client).ConfigureAwait(false);
            if (!result)
            {
                throw new IdentityServerManagerException(ErrorCodes.InternalErrorCode,
                    ErrorDescriptions.TheClientCannotBeUpdated);
            }

            _managerEventSource.FinishToUpdateClient(JsonConvert.SerializeObject(updateClientParameter));
            return result;
        }
    }
}
