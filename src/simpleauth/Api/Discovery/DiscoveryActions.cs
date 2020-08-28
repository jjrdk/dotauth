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
    using Authorization;
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using Shared.Repositories;
    using Shared.Responses;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class DiscoveryActions
    {
        private readonly IScopeRepository _scopeRepository;
        private readonly string _version;

        public DiscoveryActions(IScopeRepository scopeRepository)
        {
            _version = GetType().Assembly.GetName().Version?.ToString(4)!;
            _scopeRepository = scopeRepository;
        }

        public async Task<DiscoveryInformation> CreateDiscoveryInformation(string issuer, CancellationToken cancellationToken)
        {
            var result = new DiscoveryInformation();
            issuer = issuer.TrimEnd('/');
            // Returns only the exposed scopes
            var scopes = await _scopeRepository.GetAll(cancellationToken).ConfigureAwait(false);
            var scopeSupportedNames = scopes != null && scopes.Any()
                ? scopes.Where(s => s.IsExposed).Select(s => s.Name).ToArray()
                : Array.Empty<string>();

            var responseTypesSupported = GetSupportedResponseTypes(CoreConstants.Supported.SupportedAuthorizationFlows);

            result.ClaimsParameterSupported = true;
            result.RequestParameterSupported = true;
            result.RequestUriParameterSupported = true;
            result.RequireRequestUriRegistration = true;
            result.ClaimsSupported = Array.Empty<string>();
            result.ScopesSupported = scopeSupportedNames;
            result.ResponseTypesSupported = responseTypesSupported;
            result.ResponseModesSupported = CoreConstants.Supported.SupportedResponseModes.ToArray();
            result.GrantTypesSupported = GrantTypes.All;
            result.SubjectTypesSupported = CoreConstants.Supported.SupportedSubjectTypes.ToArray();
            result.TokenEndpointAuthMethodSupported = CoreConstants.Supported.SupportedTokenEndPointAuthenticationMethods;
            result.IdTokenSigningAlgValuesSupported = new[] { SecurityAlgorithms.RsaSha256, SecurityAlgorithms.EcdsaSha256 };
            result.IdTokenEncryptionEncValuesSupported = Array.Empty<string>();
            result.ClaimsLocalesSupported = new[] { "en" };
            result.UiLocalesSupported = new[] { "en" };
            result.Version = _version;

            // default : implement the session management : http://openid.net/specs/openid-connect-session-1_0.html

            result.Issuer = new Uri(issuer);
            result.AuthorizationEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.Authorization);
            result.TokenEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.Token);
            result.UserInfoEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.UserInfo);
            result.JwksUri = new Uri(issuer + "/" + CoreConstants.EndPoints.Jwks);
            result.RegistrationEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.Clients);
            result.RevocationEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.Revocation);
            result.IntrospectionEndpoint = new Uri(issuer + "/" + CoreConstants.EndPoints.Introspection);
            result.Jws = new Uri(issuer + "/" + CoreConstants.EndPoints.Jws);
            result.Jwe = new Uri(issuer + "/" + CoreConstants.EndPoints.Jwe);
            result.Clients = new Uri(issuer + "/" + CoreConstants.EndPoints.Clients);
            result.Scopes = new Uri(issuer + "/" + CoreConstants.EndPoints.Scopes);
            result.ResourceOwners = new Uri(issuer + "/" + CoreConstants.EndPoints.ResourceOwners);
            result.Manage = new Uri(issuer + "/" + CoreConstants.EndPoints.Manage);
            result.Claims = new Uri(issuer + "/" + CoreConstants.EndPoints.Claims);
            result.CheckSessionEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.CheckSession);
            result.EndSessionEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.EndSession);

            return result;
        }

        private static string[] GetSupportedResponseTypes(ICollection<AuthorizationFlow> authorizationFlows)
        {
            return CoreConstants.MappingResponseTypesToAuthorizationFlows
                .Where(mapping => authorizationFlows.Contains(mapping.Value))
                .Select(mapping => string.Join(" ", mapping.Key.Where(ResponseTypeNames.All.Contains)))
                .ToArray();
        }
    }
}
