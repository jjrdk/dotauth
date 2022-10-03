﻿// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
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

namespace DotAuth.Client;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;

/// <summary>
/// Defines the UMA permission client interface.
/// </summary>
public interface IUmaPermissionClient
{
    /// <summary>
    /// Gets the <see cref="Uri"/> of the UMA authority.
    /// </summary>
    Uri Authority { get; }

    /// <summary>
    /// Adds the permissions.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <param name="requests">The requests.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">
    /// requests
    /// or
    /// token
    /// </exception>
    Task<Option<TicketResponse>> RequestPermission(string token, CancellationToken cancellationToken = default, params PermissionRequest[] requests);

    /// <summary>
    /// Gets the <see cref="UmaConfiguration"/>.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<UmaConfiguration> GetUmaDocument(CancellationToken cancellationToken = default);
}