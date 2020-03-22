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
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the UMA configuration controller.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(UmaConstants.RouteValues.Configuration)]
    public class UmaConfigurationController : ControllerBase
    {
        private readonly IScopeStore _scopeStore;

        private static readonly string[] UmaProfilesSupported = new[]
        {
            "https://docs.kantarainitiative.org/uma/profiles/uma-token-bearer-1.0"
        };

        // OAUTH2.0
        private const string AuthorizationApi = "/authorization";
        private const string RegistrationApi = "/registration";
        private const string IntrospectionApi = "/introspect";
        //private const string PolicyApi = "/policies";
        private const string RevocationApi = "/token/revoke";

        /// <summary>
        /// Initializes a new instance of the <see cref="UmaConfigurationController"/> class.
        /// </summary>
        /// <param name="scopeStore"></param>
        public UmaConfigurationController(IScopeStore scopeStore)
        {
            _scopeStore = scopeStore;
        }

        /// <summary>
        /// Handles the GET configuration request.
        /// </summary>
        /// <returns>The configured <see cref="UmaConfiguration"/>.</returns>
        [HttpGet]
        public async Task<ActionResult<UmaConfiguration>> GetConfiguration(CancellationToken cancellationToken)
        {
            var absoluteUriWithVirtualPath = Request.GetAbsoluteUriWithVirtualPath();
            var scopes = await _scopeStore.GetAll(cancellationToken).ConfigureAwait(false);
            var scopeSupportedNames = scopes != null && scopes.Any()
                ? scopes.Where(s => s.IsExposed).Select(s => s.Name).ToArray()
                : Array.Empty<string>();
            var result = new UmaConfiguration
            {
                ClaimTokenProfilesSupported = Array.Empty<string>(),
                UmaProfilesSupported = UmaProfilesSupported,
                ResourceRegistrationEndpoint = absoluteUriWithVirtualPath + UmaConstants.RouteValues.ResourceSet,
                PermissionEndpoint = absoluteUriWithVirtualPath + UmaConstants.RouteValues.Permission,
                ScopesSupported = scopeSupportedNames,
                //PoliciesEndpoint = absoluteUriWithVirtualPath + PolicyApi,
                // OAUTH2.0
                Issuer = absoluteUriWithVirtualPath,
                AuthorizationEndpoint = absoluteUriWithVirtualPath + AuthorizationApi,
                TokenEndpoint = absoluteUriWithVirtualPath + UmaConstants.RouteValues.Token,
                JwksUri = absoluteUriWithVirtualPath + UmaConstants.RouteValues.Jwks,
                RegistrationEndpoint = absoluteUriWithVirtualPath + UmaConstants.RouteValues.Registration,
                IntrospectionEndpoint = absoluteUriWithVirtualPath + UmaConstants.RouteValues.Introspection,
                RevocationEndpoint = absoluteUriWithVirtualPath + RevocationApi,
                UiLocalesSupported = new[] { "en" }
            };

            return new OkObjectResult(result);

        }
    }
}
