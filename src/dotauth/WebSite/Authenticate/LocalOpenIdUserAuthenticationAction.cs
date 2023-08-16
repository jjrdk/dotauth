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

namespace DotAuth.WebSite.Authenticate;

using System;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Events;
using DotAuth.Extensions;
using DotAuth.Parameters;
using DotAuth.Properties;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Logging;

internal sealed class LocalOpenIdUserAuthenticationAction
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
        IEventPublisher eventPublisher,
        ILogger logger)
    {
        _resourceOwnerServices = resourceOwnerServices;
        _authenticateHelper = new AuthenticateHelper(
            authorizationCodeStore,
            tokenStore,
            scopeRepository,
            consentRepository,
            clientStore,
            jwksStore,
            eventPublisher,
            logger);
    }

    /// <summary>
    /// Authenticate local user account.
    /// </summary>
    /// <param name="localAuthenticationParameter">User's credentials</param>
    /// <param name="authorizationParameter">Authorization parameters</param>
    /// <param name="code">Encrypted &amp; signed authorization parameters</param>
    /// <param name="issuerName"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>Consent screen or redirect to the Index page.</returns>
    public async Task<LocalOpenIdAuthenticationResult> Execute(
        LocalAuthenticationParameter localAuthenticationParameter,
        AuthorizationParameter authorizationParameter,
        string code,
        string issuerName,
        CancellationToken cancellationToken)
    {
        var resourceOwner =
            (localAuthenticationParameter.UserName == null || localAuthenticationParameter.Password == null)
                ? null
                : await _resourceOwnerServices.Authenticate(
                        localAuthenticationParameter.UserName,
                        localAuthenticationParameter.Password,
                        cancellationToken,
                        authorizationParameter.AmrValues)
                    .ConfigureAwait(false);
        if (resourceOwner == null)
        {
            return new LocalOpenIdAuthenticationResult
            {
                ErrorMessage = Strings.TheResourceOwnerCredentialsAreNotCorrect
            };
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
