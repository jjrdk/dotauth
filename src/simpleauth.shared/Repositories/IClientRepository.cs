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
/// Defines the client repository interface.
/// </summary>
/// <seealso cref="IClientStore" />
public interface IClientRepository : IClientStore
{
    /// <summary>
    /// Searches the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<PagedResult<Client>> Search(SearchClientsRequest parameter, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the specified client.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<Option> Update(Client client, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts the specified client.
    /// </summary>
    /// <param name="client">The client.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> Insert(Client client, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified client identifier.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> Delete(string clientId, CancellationToken cancellationToken);
}