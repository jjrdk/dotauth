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

namespace DotAuth.Controllers;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Extensions;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Defines the UMA configuration controller.
/// </summary>
/// <seealso cref="ControllerBase" />
[Route(UmaConstants.RouteValues.Configuration)]
[ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, NoStore = false)]
public sealed class UmaConfigurationController : ControllerBase
{
    private readonly IScopeStore _scopeStore;

    private static readonly string[] UmaProfilesSupported =
    [
        "https://docs.kantarainitiative.org/uma/profiles/uma-token-bearer-1.0"
    ];

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
            : [];
        var result = new UmaConfiguration
        {
            ClaimTokenProfilesSupported = [],
            UmaProfilesSupported = UmaProfilesSupported,
            ResourceRegistrationEndpoint = new Uri(
                $"{absoluteUriWithVirtualPath}/{UmaConstants.RouteValues.ResourceSet}"),
            PermissionEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{UmaConstants.RouteValues.Permission}"),
            ScopesSupported = scopeSupportedNames,
            //PoliciesEndpoint = absoluteUriWithVirtualPath + PolicyApi,
            // OAUTH2.0
            Issuer = new Uri(absoluteUriWithVirtualPath),
            AuthorizationEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{CoreConstants.EndPoints.Authorization}"),
            TokenEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{CoreConstants.EndPoints.Token}"),
            JwksUri = new Uri($"{absoluteUriWithVirtualPath}/{CoreConstants.EndPoints.Jwks}"),
            RegistrationEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{CoreConstants.EndPoints.Clients}"),
            IntrospectionEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{UmaConstants.RouteValues.Introspection}"),
            RevocationEndpoint = new Uri($"{absoluteUriWithVirtualPath}/{CoreConstants.EndPoints.Token}/revoke"),
            UiLocalesSupported = ["en"],
            GrantTypesSupported = GrantTypes.All,
            ResponseTypesSupported = ResponseTypeNames.All
        };

        return new OkObjectResult(result);

    }
}