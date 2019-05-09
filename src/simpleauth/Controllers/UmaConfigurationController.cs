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

namespace SimpleAuth.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the UMA configuration controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(UmaConstants.RouteValues.Configuration)]
    public class UmaConfigurationController : ControllerBase
    {
        private readonly List<string> _umaProfilesSupported = new List<string>
        {
            "https://docs.kantarainitiative.org/uma/profiles/uma-token-bearer-1.0"
        };

        private const string ResourceSetApi = "/rs/resource_set";
        private const string PermissionApi = "/perm";

        // OAUTH2.0
        private const string AuthorizationApi = "/authorization";
        private const string TokenApi = "/token";
        private const string JwksApi = "/jwks";
        private const string RegistrationApi = "/registration";
        private const string IntrospectionApi = "/introspect";
        private const string PolicyApi = "/policies";
        private const string RevocationApi = "/token/revoke";

        /// <summary>
        /// Handles the GET configuration request.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<UmaConfigurationResponse>> GetConfiguration()
        {
            var result = (await GetConfiguration(Request).ConfigureAwait(false));//.ToResponse();
            return new OkObjectResult(result);
        }

        private Task<UmaConfigurationResponse> GetConfiguration(HttpRequest request)
        {
            var absoluteUriWithVirtualPath = request.GetAbsoluteUriWithVirtualPath();
            var result = new UmaConfigurationResponse
            {
                ClaimTokenProfilesSupported = new List<string>(),
                UmaProfilesSupported = _umaProfilesSupported,
                ResourceRegistrationEndpoint = absoluteUriWithVirtualPath + ResourceSetApi,
                PermissionEndpoint = absoluteUriWithVirtualPath + PermissionApi,
                PoliciesEndpoint = absoluteUriWithVirtualPath + PolicyApi,
                // OAUTH2.0
                Issuer = absoluteUriWithVirtualPath,
                AuthorizationEndpoint = absoluteUriWithVirtualPath + AuthorizationApi,
                TokenEndpoint = absoluteUriWithVirtualPath + TokenApi,
                JwksUri = absoluteUriWithVirtualPath + JwksApi,
                RegistrationEndpoint = absoluteUriWithVirtualPath + RegistrationApi,
                IntrospectionEndpoint = absoluteUriWithVirtualPath + IntrospectionApi,
                RevocationEndpoint = absoluteUriWithVirtualPath + RevocationApi
            };

            return Task.FromResult(result);
        }
    }
}
