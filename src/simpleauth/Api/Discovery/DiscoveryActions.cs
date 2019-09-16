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
            _version = GetType().Assembly.GetName().Version.ToString(4);
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

            var grantTypesSupported = CoreConstants.Supported.SupportedGrantTypes;

            result.ClaimsParameterSupported = true;
            result.RequestParameterSupported = true;
            result.RequestUriParameterSupported = true;
            result.RequireRequestUriRegistration = true;
            result.ClaimsSupported = Array.Empty<string>(); //(await _claimRepository.GetAll().ConfigureAwait(false)).ToArray();
            result.ScopesSupported = scopeSupportedNames;
            result.ResponseTypesSupported = responseTypesSupported;
            result.ResponseModesSupported = CoreConstants.Supported.SupportedResponseModes.ToArray();
            result.GrantTypesSupported = grantTypesSupported;
            result.SubjectTypesSupported = CoreConstants.Supported.SupportedSubjectTypes.ToArray();
            result.TokenEndpointAuthMethodSupported = CoreConstants.Supported.SupportedTokenEndPointAuthenticationMethods;
            result.IdTokenSigningAlgValuesSupported = new[] { SecurityAlgorithms.RsaSha256, SecurityAlgorithms.EcdsaSha256 };
            result.IdTokenEncryptionEncValuesSupported = Array.Empty<string>();
            result.ClaimsLocalesSupported = new[] { "en" };
            result.UiLocalesSupported = new[] { "en" };
            result.Version = _version;
            //var issuer = Request.GetAbsoluteUriWithVirtualPath();

            // default : implement the session management : http://openid.net/specs/openid-connect-session-1_0.html

            result.Issuer = issuer;
            result.AuthorizationEndPoint = issuer + "/" + CoreConstants.EndPoints.Authorization;
            result.TokenEndPoint = issuer + "/" + CoreConstants.EndPoints.Token;
            result.UserInfoEndPoint = issuer + "/" + CoreConstants.EndPoints.UserInfo;
            result.JwksUri = issuer + "/" + CoreConstants.EndPoints.Jwks;
            result.RegistrationEndPoint = issuer + "/" + CoreConstants.EndPoints.Registration;
            result.RevocationEndPoint = issuer + "/" + CoreConstants.EndPoints.Revocation;
            result.IntrospectionEndPoint = issuer + "/" + CoreConstants.EndPoints.Introspection;
            result.Jws = issuer + "/" + CoreConstants.EndPoints.Jws;
            result.Jwe = issuer + "/" + CoreConstants.EndPoints.Jwe;
            result.Clients = issuer + "/" + CoreConstants.EndPoints.Clients;
            result.Scopes = issuer + "/" + CoreConstants.EndPoints.Scopes;
            result.ResourceOwners = issuer + "/" + CoreConstants.EndPoints.ResourceOwners;
            result.Manage = issuer + "/" + CoreConstants.EndPoints.Manage;
            result.Claims = issuer + "/" + CoreConstants.EndPoints.Claims;
            result.CheckSessionEndPoint = issuer + "/" + CoreConstants.EndPoints.CheckSession;
            result.EndSessionEndPoint = issuer + "/" + CoreConstants.EndPoints.EndSession;

            return result;
        }

        private static string[] GetSupportedResponseTypes(ICollection<AuthorizationFlow> authorizationFlows)
        {
            var result = new List<string>();
            foreach (var mapping in CoreConstants.MappingResponseTypesToAuthorizationFlows)
            {
                if (authorizationFlows.Contains(mapping.Value))
                {
                    var record = string.Join(" ", mapping.Key.Where(ResponseTypeNames.All.Contains));
                    //.Select(k => Enum.GetName(typeof(string), k)));
                    result.Add(record);
                }
            }

            return result.ToArray();
        }

        //private static string[] GetSupportedGrantTypes()
        //{
        //    var result = new List<string>();
        //    foreach (var supportedGrantType in )
        //    {
        //        //var record = Enum.GetName(typeof(GrantType), supportedGrantType);
        //        result.Add(supportedGrantType);
        //    }

        //    return result.ToArray();
        //}

        //private static string[] GetSupportedTokenEndPointAuthMethods()
        //{
        //    var result = new List<string>();
        //    foreach (var supportedAuthMethod in CoreConstants.Supported.SupportedTokenEndPointAuthenticationMethods)
        //    {
        //        var record = Enum.GetName(typeof(TokenEndPointAuthenticationMethods), supportedAuthMethod);
        //        result.Add(record);
        //    }

        //    return result.ToArray();
        //}
    }
}
