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
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    public interface IPolicyClient
    {
        /// <summary>
        /// Adds the policy.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="accessToken">The authorization header value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// authorizationHeaderValue
        /// </exception>
        Task<GenericResponse<AddPolicyResponse>> AddPolicy(PolicyData request, string accessToken);

        /// <summary>
        /// Gets the policy.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// id
        /// or
        /// token
        /// </exception>
        Task<GenericResponse<PolicyResponse>> GetPolicy(string id, string token);

        /// <summary>
        /// Gets all policies.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">token</exception>
        Task<GenericResponse<string[]>> GetAllPolicies(string token);

        /// <summary>
        /// Deletes the policy.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// id
        /// or
        /// token
        /// </exception>
        Task<GenericResponse<object>> DeletePolicy(string id, string token);

        /// <summary>
        /// Updates the policy.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// token
        /// </exception>
        Task<GenericResponse<object>> UpdatePolicy(PolicyData request, string token);

        /// <summary>
        /// Searches the policies.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="authorizationHeaderValue">The authorization header value.</param>
        /// <returns></returns>
        Task<GenericResponse<SearchAuthPoliciesResponse>> SearchPolicies(
            SearchAuthPolicies parameter,
            string authorizationHeaderValue = null);
    }
}
