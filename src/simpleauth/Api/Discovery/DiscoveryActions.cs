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

namespace SimpleAuth.Api.Discovery;

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

internal sealed class DiscoveryActions
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
        issuer = issuer.TrimEnd('/');
        // Returns only the exposed scopes
        var scopes = await _scopeRepository.GetAll(cancellationToken).ConfigureAwait(false);
        var scopeSupportedNames = scopes != null && scopes.Any()
            ? scopes.Where(s => s.IsExposed).Select(s => s.Name).ToArray()
            : Array.Empty<string>();

        var responseTypesSupported = GetSupportedResponseTypes(CoreConstants.Supported.SupportedAuthorizationFlows);

        var result = new DiscoveryInformation
        {
            ClaimsParameterSupported = true,
            RequestParameterSupported = true,
            RequestUriParameterSupported = true,
            RequireRequestUriRegistration = true,
            ClaimsSupported = Array.Empty<string>(),
            ScopesSupported = scopeSupportedNames,
            ResponseTypesSupported = responseTypesSupported,
            ResponseModesSupported = CoreConstants.Supported.SupportedResponseModes.ToArray(),
            GrantTypesSupported = GrantTypes.All,
            SubjectTypesSupported = CoreConstants.Supported.SupportedSubjectTypes.ToArray(),
            TokenEndpointAuthMethodSupported = CoreConstants.Supported.SupportedTokenEndPointAuthenticationMethods,
            IdTokenSigningAlgValuesSupported = new[] {SecurityAlgorithms.RsaSha256, SecurityAlgorithms.EcdsaSha256},
            IdTokenEncryptionEncValuesSupported = Array.Empty<string>(),
            ClaimsLocalesSupported = new[] {"en"},
            UiLocalesSupported = new[] {"en"},
            Version = _version,

            // default : implement the session management : http://openid.net/specs/openid-connect-session-1_0.html

            Issuer = new Uri(issuer),
            DeviceAuthorizationEndPoint = new Uri(issuer+"/"+CoreConstants.EndPoints.DeviceAuthorization),
            AuthorizationEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.Authorization),
            TokenEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.Token),
            UserInfoEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.UserInfo),
            JwksUri = new Uri(issuer + "/" + CoreConstants.EndPoints.Jwks),
            RegistrationEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.Clients),
            RevocationEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.Revocation),
            IntrospectionEndpoint = new Uri(issuer + "/" + CoreConstants.EndPoints.Introspection),
            Jws = new Uri(issuer + "/" + CoreConstants.EndPoints.Jws),
            Jwe = new Uri(issuer + "/" + CoreConstants.EndPoints.Jwe),
            Clients = new Uri(issuer + "/" + CoreConstants.EndPoints.Clients),
            Scopes = new Uri(issuer + "/" + CoreConstants.EndPoints.Scopes),
            ResourceOwners = new Uri(issuer + "/" + CoreConstants.EndPoints.ResourceOwners),
            Manage = new Uri(issuer + "/" + CoreConstants.EndPoints.Manage),
            Claims = new Uri(issuer + "/" + CoreConstants.EndPoints.Claims),
            CheckSessionEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.CheckSession),
            EndSessionEndPoint = new Uri(issuer + "/" + CoreConstants.EndPoints.EndSession),
        };
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