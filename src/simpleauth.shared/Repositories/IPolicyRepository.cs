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

namespace SimpleAuth.Shared.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the policy repository interface.
    /// </summary>
    public interface IPolicyRepository
    {
        /// <summary>
        /// Searches the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<GenericResult<Policy>> Search(SearchAuthPolicies parameter, CancellationToken cancellationToken);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="owner">The policy owner.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Policy[]> GetAll(string owner, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Policy> Get(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Adds the specified policy.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> Add(Policy policy, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> Delete(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the specified policy.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<bool> Update(Policy policy, CancellationToken cancellationToken);
    }
}
