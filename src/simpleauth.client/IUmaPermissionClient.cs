// Copyright © 2016 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Client
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Responses;

    public interface IUmaPermissionClient
    {
        /// <summary>
        /// Adds the permission.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// token
        /// </exception>
        Task<GenericResponse<PermissionResponse>> RequestPermission(string token, PermissionRequest request);

        /// <summary>
        /// Adds the permissions.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="requests">The requests.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// requests
        /// or
        /// token
        /// </exception>
        Task<GenericResponse<PermissionResponse>> RequestPermissions(string token, params PermissionRequest[] requests);
    }
}
