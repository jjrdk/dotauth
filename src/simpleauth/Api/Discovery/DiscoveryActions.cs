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

namespace SimpleAuth.Api.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Authorization;
    using Shared.Models;
    using Shared.Repositories;
    using Shared.Responses;

    public class DiscoveryActions : IDiscoveryActions
    {
        private readonly IScopeRepository _scopeRepository;
        private readonly IClaimRepository _claimRepository;

        public DiscoveryActions(IScopeRepository scopeRepository, IClaimRepository claimRepository)
        {
            _scopeRepository = scopeRepository;
            _claimRepository = claimRepository;
        }

        public async Task<DiscoveryInformation> CreateDiscoveryInformation(string issuer, string scimEndpoint = null)
        {
            var result = new DiscoveryInformation();
            issuer = issuer.TrimEnd('/');
            // Returns only the exposed scopes
            var scopes = await _scopeRepository.GetAll().ConfigureAwait(false);
            var scopeSupportedNames = new string[0];
            if (scopes != null ||
                scopes.Any())
            {
                scopeSupportedNames = scopes.Where(s => s.IsExposed).Select(s => s.Name).ToArray();
            }

            var responseTypesSupported = GetSupportedResponseTypes(CoreConstants.Supported.SupportedAuthorizationFlows);

            var grantTypesSupported = GetSupportedGrantTypes();
            var tokenAuthMethodSupported = GetSupportedTokenEndPointAuthMethods();

            result.ClaimsParameterSupported = true;
            result.RequestParameterSupported = true;
            result.RequestUriParameterSupported = true;
            result.RequireRequestUriRegistration = true;
            result.ClaimsSupported = (await _claimRepository.GetAllAsync().ConfigureAwait(false)).Select(c => c.Code).ToArray();
            result.ScopesSupported = scopeSupportedNames;
            result.ResponseTypesSupported = responseTypesSupported;
            result.ResponseModesSupported = CoreConstants.Supported.SupportedResponseModes.ToArray();
            result.GrantTypesSupported = grantTypesSupported;
            result.SubjectTypesSupported = CoreConstants.Supported.SupportedSubjectTypes.ToArray();
            result.TokenEndpointAuthMethodSupported = tokenAuthMethodSupported;
            result.IdTokenSigningAlgValuesSupported = CoreConstants.Supported.SupportedJwsAlgs.ToArray();
            //var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var authorizationEndPoint = issuer + "/" + CoreConstants.EndPoints.Authorization;
            var tokenEndPoint = issuer + "/" + CoreConstants.EndPoints.Token;
            var userInfoEndPoint = issuer + "/" + CoreConstants.EndPoints.UserInfo;
            var jwksUri = issuer + "/" + CoreConstants.EndPoints.Jwks;
            var registrationEndPoint = issuer + "/" + CoreConstants.EndPoints.Registration;
            var revocationEndPoint = issuer + "/" + CoreConstants.EndPoints.Revocation;
            // TODO : implement the session management : http://openid.net/specs/openid-connect-session-1_0.html
            var checkSessionIframe = issuer + "/" + CoreConstants.EndPoints.CheckSession;
            var endSessionEndPoint = issuer + "/" + CoreConstants.EndPoints.EndSession;
            var introspectionEndPoint = issuer + "/" + CoreConstants.EndPoints.Introspection;

            result.Issuer = issuer;
            result.AuthorizationEndPoint = authorizationEndPoint;
            result.TokenEndPoint = tokenEndPoint;
            result.UserInfoEndPoint = userInfoEndPoint;
            result.JwksUri = jwksUri;
            result.RegistrationEndPoint = registrationEndPoint;
            result.RevocationEndPoint = revocationEndPoint;
            result.IntrospectionEndPoint = introspectionEndPoint;
            result.Jws = issuer + "/" + CoreConstants.EndPoints.Jws;
            result.Jwe = issuer + "/" + CoreConstants.EndPoints.Jwe;
            result.Clients = issuer + "/" + CoreConstants.EndPoints.Clients;
            result.Scopes = issuer + "/" + CoreConstants.EndPoints.Scopes;
            result.ResourceOwners = issuer + "/" + CoreConstants.EndPoints.ResourceOwners;
            result.Manage = issuer + "/" + CoreConstants.EndPoints.Manage;
            result.Claims = issuer + "/" + CoreConstants.EndPoints.Claims;
            result.Version = "1.0";
            result.CheckSessionEndPoint = checkSessionIframe;
            result.EndSessionEndPoint = endSessionEndPoint;
            if (!string.IsNullOrWhiteSpace(scimEndpoint))
            {
                result.ScimEndpoint = scimEndpoint;
            }

            return result;
        }

        private static string[] GetSupportedResponseTypes(ICollection<AuthorizationFlow> authorizationFlows)
        {
            var result = new List<string>();
            foreach (var mapping in CoreConstants.MappingResponseTypesToAuthorizationFlows)
            {
                if (authorizationFlows.Contains(mapping.Value))
                {
                    var record = string.Join(" ", mapping.Key.Select(k => Enum.GetName(typeof(ResponseType), k)));
                    result.Add(record);
                }
            }

            return result.ToArray();
        }

        private static string[] GetSupportedGrantTypes()
        {
            var result = new List<string>();
            foreach (var supportedGrantType in CoreConstants.Supported.SupportedGrantTypes)
            {
                var record = Enum.GetName(typeof(GrantType), supportedGrantType);
                result.Add(record);
            }

            return result.ToArray();
        }

        private static string[] GetSupportedTokenEndPointAuthMethods()
        {
            var result = new List<string>();
            foreach (var supportedAuthMethod in CoreConstants.Supported.SupportedTokenEndPointAuthenticationMethods)
            {
                var record = Enum.GetName(typeof(TokenEndPointAuthenticationMethods), supportedAuthMethod);
                result.Add(record);
            }

            return result.ToArray();
        }

    }
}
