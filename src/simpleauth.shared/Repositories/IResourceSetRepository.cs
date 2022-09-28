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

namespace SimpleAuth.Shared.Repositories;

using System.Threading;
using System.Threading.Tasks;
using SimpleAuth.Shared.Models;
using SimpleAuth.Shared.Requests;

/// <summary>
/// Defines the resource set repository interface.
/// </summary>
public interface IResourceSetRepository
{
    /// <summary>
    /// Searches the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<PagedResult<ResourceSet>> Search(SearchResourceSet parameter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts the specified resource set.
    /// </summary>
    /// <param name="owner">The resource owner.</param>
    /// <param name="resourceSet">The resource set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<bool> Add(string owner, ResourceSet resourceSet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the specified identifier.
    /// </summary>
    /// <param name="owner">The resource owner.</param>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<ResourceSet?> Get(string owner, string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the specified ids.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <param name="ids">The ids.</param>
    /// <returns></returns>
    Task<ResourceSet[]> Get(CancellationToken cancellationToken = default, params string[] ids);

    /// <summary>
    /// Gets the owner for the requested resource ids.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <param name="ids">The ids.</param>
    /// <returns>The owner subject if all resources have the same owner, otherwise <c>null</c>.</returns>
    Task<string?> GetOwner(CancellationToken cancellationToken = default, params string[] ids);

    /// <summary>
    /// Updates the specified resource set.
    /// </summary>
    /// <param name="resourceSet">The resource set.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option> Update(ResourceSet resourceSet, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<ResourceSet[]> GetAll(string owner, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<bool> Remove(string id, CancellationToken cancellationToken = default);
}