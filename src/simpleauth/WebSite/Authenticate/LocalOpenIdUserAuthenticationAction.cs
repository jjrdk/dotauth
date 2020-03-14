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

namespace SimpleAuth.WebSite.Authenticate
{
    using SimpleAuth.Extensions;
    using SimpleAuth.Parameters;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Errors;
    using SimpleAuth.Shared.Repositories;
    using System;
    using System.Globalization;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    internal class LocalOpenIdUserAuthenticationAction
    {
        private readonly IAuthenticateResourceOwnerService[] _resourceOwnerServices;
        private readonly AuthenticateHelper _authenticateHelper;

        public LocalOpenIdUserAuthenticationAction(
            IAuthorizationCodeStore authorizationCodeStore,
            IAuthenticateResourceOwnerService[] resourceOwnerServices,
            IConsentRepository consentRepository,
            ITokenStore tokenStore,
            IScopeRepository scopeRepository,
            IClientStore clientStore,
            IJwksStore jwksStore,
            IEventPublisher eventPublisher)
        {
            _resourceOwnerServices = resourceOwnerServices;
            _authenticateHelper = new AuthenticateHelper(
                authorizationCodeStore,
                tokenStore,
                scopeRepository,
                consentRepository,
                clientStore,
                jwksStore,
                eventPublisher);
        }

        /// <summary>
        /// Authenticate local user account.
        /// Exceptions :
        /// Throw the exception <see cref="SimpleAuthException "/> if the user cannot be authenticated
        /// </summary>
        /// <param name="localAuthenticationParameter">User's credentials</param>
        /// <param name="authorizationParameter">Authorization parameters</param>
        /// <param name="code">Encrypted &amp; signed authorization parameters</param>
        /// <param name="issuerName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Consent screen or redirect to the Index page.</returns>
        public async Task<LocalOpenIdAuthenticationResult> Execute(
            LocalAuthenticationParameter localAuthenticationParameter,
            AuthorizationParameter authorizationParameter,
            string code,
            string issuerName,
            CancellationToken cancellationToken)
        {
            var resourceOwner = await _resourceOwnerServices.Authenticate(
                    localAuthenticationParameter.UserName,
                    localAuthenticationParameter.Password,
                    cancellationToken,
                    authorizationParameter.AmrValues)
                .ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(
                    ErrorCodes.InvalidRequest,
                    "The resource owner credentials are not correct");
            }

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.AuthenticationInstant,
                    DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                    ClaimValueTypes.Integer)
            }.Add(resourceOwner.Claims);

            return new LocalOpenIdAuthenticationResult
            {
                EndpointResult =
                    await _authenticateHelper.ProcessRedirection(
                            authorizationParameter,
                            code,
                            resourceOwner.Subject,
                            claims,
                            issuerName,
                            cancellationToken)
                        .ConfigureAwait(false),
                Claims = claims,
                TwoFactor = resourceOwner.TwoFactorAuthentication
            };
        }
    }
}
