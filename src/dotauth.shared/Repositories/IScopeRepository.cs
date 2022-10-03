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

/// <summary>
/// Defines the scope repository interface.
/// </summary>
/// <seealso cref="IScopeStore" />
public interface IScopeRepository : IScopeStore
{
    /// <summary>
    /// Inserts the specified scope.
    /// </summary>
    /// <param name="scope">The scope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> Insert(Scope scope, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified scope.
    /// </summary>
    /// <param name="scope">The scope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> Delete(Scope scope, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the specified scope.
    /// </summary>
    /// <param name="scope">The scope.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> Update(Scope scope, CancellationToken cancellationToken);
}