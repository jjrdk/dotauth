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

namespace SimpleAuth.Uma.Api.ConfigurationController.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Responses;
    using SimpleAuth;

    public class GetConfigurationAction : IGetConfigurationAction
    {
        private readonly IHttpContextAccessor _hostingProvider;

        private readonly List<string> _umaProfilesSupported = new List<string>
        {
            "https://docs.kantarainitiative.org/uma/profiles/uma-token-bearer-1.0"
        };
        private const string ResourceSetApi = "/rs/resource_set";
        private const string PermissionApi = "/perm";
        // OAUTH2.0
        //private const string AuthorizationApi = "/authorization";
        private const string TokenApi = "/token";
        private const string JwksApi = "/jwks";
        private const string RegistrationApi = "/registration";
        private const string IntrospectionApi = "/introspect";
        private const string PolicyApi = "/policies";
        private const string RevocationApi = "/token/revoke";

        public GetConfigurationAction(IHttpContextAccessor hostingProvider)
        {
            _hostingProvider = hostingProvider;
        }
        
        public Task<ConfigurationResponse> Execute()
        {
            var absoluteUriWithVirtualPath = _hostingProvider.GetAbsoluteUriWithVirtualPath();
            var result = new ConfigurationResponse
            {
                ClaimTokenProfilesSupported = new List<string>(),
                UmaProfilesSupported = _umaProfilesSupported,
                ResourceRegistrationEndpoint = absoluteUriWithVirtualPath + ResourceSetApi,
                PermissionEndpoint = absoluteUriWithVirtualPath + PermissionApi,
                PoliciesEndpoint = absoluteUriWithVirtualPath + PolicyApi,
                // OAUTH2.0
                Issuer = absoluteUriWithVirtualPath,
                AuthorizationEndpoint = null,// absoluteUriWithVirtualPath + AuthorizationApi,
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