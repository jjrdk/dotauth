// Copyright 2016 Habart Thierry
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

namespace SimpleAuth.Uma.Client.Permission
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Configuration;
    using Helpers;
    using Results;
    using Shared.DTOs;

    internal class PermissionClient : IPermissionClient
    {
        private readonly IAddPermissionsOperation _addPermissionsOperation;
        private readonly IGetConfigurationOperation _getConfigurationOperation;

        public PermissionClient(
            IAddPermissionsOperation addPermissionsOperation,
            IGetConfigurationOperation getConfigurationOperation)
        {
            _addPermissionsOperation = addPermissionsOperation;
            _getConfigurationOperation = getConfigurationOperation;
        }

        public Task<AddPermissionResult> Add(PostPermission request, string url, string token)
        {
            return _addPermissionsOperation.ExecuteAsync(request, url, token);
        }

        public async Task<AddPermissionResult> AddByResolution(PostPermission request, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Add(request, configuration.PermissionEndpoint, token).ConfigureAwait(false);
        }

        public Task<AddPermissionResult> Add(IEnumerable<PostPermission> request, string url, string token)
        {
            return _addPermissionsOperation.ExecuteAsync(request, url, token);
        }

        public async Task<AddPermissionResult> AddByResolution(IEnumerable<PostPermission> request, string url, string token)
        {
            var configuration = await _getConfigurationOperation.ExecuteAsync(UriHelpers.GetUri(url)).ConfigureAwait(false);
            return await Add(request, configuration.PermissionEndpoint, token).ConfigureAwait(false);
        }
    }
}
