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

namespace DotAuth.Services;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the authenticate resource owner service interface.
/// </summary>
public interface IAuthenticateResourceOwnerService
{
    /// <summary>
    /// Authenticates the resource owner.
    /// </summary>
    /// <param name="login">The login.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<ResourceOwner?> AuthenticateResourceOwner(
        string login,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the amr.
    /// </summary>
    /// <value>
    /// The amr.
    /// </value>
    string Amr { get; }
}