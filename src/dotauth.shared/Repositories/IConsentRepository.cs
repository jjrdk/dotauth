﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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
/// Defines the consent repository.
/// </summary>
/// <seealso cref="IConsentStore" />
public interface IConsentRepository : IConsentStore
{
    /// <summary>
    /// Inserts the specified record.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> Insert(Consent record, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified record.
    /// </summary>
    /// <param name="record">The record.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> Delete(Consent record, CancellationToken cancellationToken);
}