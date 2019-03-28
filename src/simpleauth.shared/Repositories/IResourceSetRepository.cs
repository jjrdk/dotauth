// Copyright � 2015 Habart Thierry, � 2018 Jacob Reimers
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
    /// Defines the resource set repository interface.
    /// </summary>
    public interface IResourceSetRepository
    {
        /// <summary>
        /// Searches the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<GenericResult<ResourceSet>> Search(SearchResourceSet parameter, CancellationToken cancellationToken);

        /// <summary>
        /// Inserts the specified resource set.
        /// </summary>
        /// <param name="resourceSet">The resource set.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Add(ResourceSet resourceSet, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ResourceSet> Get(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the specified resource set.
        /// </summary>
        /// <param name="resourceSet">The resource set.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Update(ResourceSet resourceSet, CancellationToken cancellationToken);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ResourceSet[]> GetAll(CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Remove(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the specified ids.
        /// </summary>
        /// <param name="ids">The ids.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<ResourceSet[]> Get(CancellationToken cancellationToken, params string[] ids);
    }
}