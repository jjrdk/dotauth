﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

using SimpleAuth.Services;

namespace SimpleAuth.WebSite.Authenticate.Actions
{
    using Common;
    using Exceptions;
    using Extensions;
    using Helpers;
    using Parameters;
    using SimpleAuth.Errors;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class LocalOpenIdUserAuthenticationAction
    {
        private readonly IEnumerable<IAuthenticateResourceOwnerService> _resourceOwnerServices;
        private readonly IAuthenticateHelper _authenticateHelper;

        public LocalOpenIdUserAuthenticationAction(
            IEnumerable<IAuthenticateResourceOwnerService> resourceOwnerServices,
            IAuthenticateHelper authenticateHelper)
        {
            _resourceOwnerServices = resourceOwnerServices;
            _authenticateHelper = authenticateHelper;
        }

        /// <summary>
        /// Authenticate local user account.
        /// Exceptions :
        /// Throw the exception <see cref="AuthServerAuthenticationException "/> if the user cannot be authenticated
        /// </summary>
        /// <param name="localAuthenticationParameter">User's credentials</param>
        /// <param name="authorizationParameter">Authorization parameters</param>
        /// <param name="code">Encrypted & signed authorization parameters</param>
        /// <param name="claims">Returned the claims of the authenticated user</param>
        /// <returns>Consent screen or redirect to the Index page.</returns>
        public async Task<LocalOpenIdAuthenticationResult> Execute(
            LocalAuthenticationParameter localAuthenticationParameter,
            AuthorizationParameter authorizationParameter,
            string code, string issuerName)
        {
            if (localAuthenticationParameter == null)
            {
                throw new ArgumentNullException(nameof(localAuthenticationParameter));
            }

            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            var resourceOwner = await _resourceOwnerServices.Authenticate(localAuthenticationParameter.UserName, localAuthenticationParameter.Password, authorizationParameter.AmrValues).ConfigureAwait(false);
            if (resourceOwner == null)
            {
                throw new SimpleAuthException(ErrorCodes.InvalidRequestCode, "The resource owner credentials are not correct");
            }

            var claims = resourceOwner.Claims == null ? new List<Claim>() : resourceOwner.Claims.ToList();
            claims.Add(new Claim(
                ClaimTypes.AuthenticationInstant,
                DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer));
            return new LocalOpenIdAuthenticationResult
            {
                EndpointResult = await _authenticateHelper.ProcessRedirection(authorizationParameter,
                    code,
                    resourceOwner.Id,
                    claims, issuerName).ConfigureAwait(false),
                Claims = claims,
                TwoFactor = resourceOwner.TwoFactorAuthentication
            };
        }
    }
}
