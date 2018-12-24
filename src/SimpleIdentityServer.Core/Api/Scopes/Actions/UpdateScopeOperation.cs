// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleIdentityServer.Core.Api.Scopes.Actions
{
    using System;
    using System.Threading.Tasks;
    using Errors;
    using Exceptions;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    internal class UpdateScopeOperation : IUpdateScopeOperation
    {
        private readonly IScopeRepository _scopeRepository;

        public UpdateScopeOperation(IScopeRepository scopeRepository)
        {
            _scopeRepository = scopeRepository;
        }
        
        public async Task<bool> Execute(Scope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (await _scopeRepository.Get(scope.Name).ConfigureAwait(false) == null)
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidParameterCode,
                    string.Format(ErrorDescriptions.TheScopeDoesntExist, scope.Name));
            }

            return await _scopeRepository.Update(scope).ConfigureAwait(false);
        }
    }
}
