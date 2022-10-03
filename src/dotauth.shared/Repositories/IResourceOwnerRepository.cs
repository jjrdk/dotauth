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

namespace DotAuth.Shared.Repositories;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;

/// <summary>
/// Defines the resource owner repository.
/// </summary>
/// <seealso cref="IResourceOwnerStore" />
public interface IResourceOwnerRepository : IResourceOwnerStore
{
    /// <summary>
    /// Gets all.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<ResourceOwner[]> GetAll(CancellationToken cancellationToken);

    /// <summary>
    /// Inserts the specified resource owner.
    /// </summary>
    /// <param name="resourceOwner">The resource owner.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> Insert(ResourceOwner resourceOwner, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the specified resource owner.
    /// </summary>
    /// <param name="resourceOwner">The resource owner.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<Option> Update(ResourceOwner resourceOwner, CancellationToken cancellationToken);

    /// <summary>
    /// Sets the password.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> SetPassword(string subject, string password, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified subject.
    /// </summary>
    /// <param name="subject">The subject.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> Delete(string subject, CancellationToken cancellationToken);

    /// <summary>
    /// Searches the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<PagedResult<ResourceOwner>> Search(SearchResourceOwnersRequest parameter, CancellationToken cancellationToken);
}