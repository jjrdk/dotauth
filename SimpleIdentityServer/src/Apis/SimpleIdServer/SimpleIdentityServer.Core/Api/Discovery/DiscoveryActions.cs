// Copyright 2015 Habart Thierry
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

using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Api.Discovery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Authorization;
    using Common.Models;
    using Common.Repositories;

    public interface IDiscoveryActions
    {
        Task<DiscoveryInformation> CreateDiscoveryInformation(string issuer, string scimEndpoint = null);
    }

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
            var scopes = await _scopeRepository.GetAllAsync().ConfigureAwait(false);
            var scopeSupportedNames = new string[0];
            if (scopes != null ||
                scopes.Any())
            {
                scopeSupportedNames = scopes.Where(s => s.IsExposed).Select(s => s.Name).ToArray();
            }

            var responseTypesSupported = GetSupportedResponseTypes(Constants.Supported.SupportedAuthorizationFlows);

            var grantTypesSupported = GetSupportedGrantTypes();
            var tokenAuthMethodSupported = GetSupportedTokenEndPointAuthMethods();

            result.ClaimsParameterSupported = true;
            result.RequestParameterSupported = true;
            result.RequestUriParameterSupported = true;
            result.RequireRequestUriRegistration = true;
            result.ClaimsSupported = (await _claimRepository.GetAllAsync().ConfigureAwait(false)).Select(c => c.Code).ToArray();
            result.ScopesSupported = scopeSupportedNames;
            result.ResponseTypesSupported = responseTypesSupported;
            result.ResponseModesSupported = Constants.Supported.SupportedResponseModes.ToArray();
            result.GrantTypesSupported = grantTypesSupported;
            result.SubjectTypesSupported = Constants.Supported.SupportedSubjectTypes.ToArray();
            result.TokenEndpointAuthMethodSupported = tokenAuthMethodSupported;
            result.IdTokenSigningAlgValuesSupported = Constants.Supported.SupportedJwsAlgs.ToArray();
            //var issuer = Request.GetAbsoluteUriWithVirtualPath();
            var authorizationEndPoint = issuer + "/" + Constants.EndPoints.Authorization;
            var tokenEndPoint = issuer + "/" + Constants.EndPoints.Token;
            var userInfoEndPoint = issuer + "/" + Constants.EndPoints.UserInfo;
            var jwksUri = issuer + "/" + Constants.EndPoints.Jwks;
            var registrationEndPoint = issuer + "/" + Constants.EndPoints.Registration;
            var revocationEndPoint = issuer + "/" + Constants.EndPoints.Revocation;
            // TODO : implement the session management : http://openid.net/specs/openid-connect-session-1_0.html
            var checkSessionIframe = issuer + "/" + Constants.EndPoints.CheckSession;
            var endSessionEndPoint = issuer + "/" + Constants.EndPoints.EndSession;
            var introspectionEndPoint = issuer + "/" + Constants.EndPoints.Introspection;

            result.Issuer = issuer;
            result.AuthorizationEndPoint = authorizationEndPoint;
            result.TokenEndPoint = tokenEndPoint;
            result.UserInfoEndPoint = userInfoEndPoint;
            result.JwksUri = jwksUri;
            result.RegistrationEndPoint = registrationEndPoint;
            result.RevocationEndPoint = revocationEndPoint;
            result.IntrospectionEndPoint = introspectionEndPoint;
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
            foreach (var mapping in Constants.MappingResponseTypesToAuthorizationFlows)
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
            foreach (var supportedGrantType in Constants.Supported.SupportedGrantTypes)
            {
                var record = Enum.GetName(typeof(GrantType), supportedGrantType);
                result.Add(record);
            }

            return result.ToArray();
        }

        private static string[] GetSupportedTokenEndPointAuthMethods()
        {
            var result = new List<string>();
            foreach (var supportedAuthMethod in Constants.Supported.SupportedTokenEndPointAuthenticationMethods)
            {
                var record = Enum.GetName(typeof(TokenEndPointAuthenticationMethods), supportedAuthMethod);
                result.Add(record);
            }

            return result.ToArray();
        }

    }
}
