// Copyright © 2018 Jacob Reimers
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

namespace DotAuth.Services;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;
using DotAuth.Shared.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// A hosted service that provisions the default tenant at application startup
/// by delegating to <see cref="ITenantProvisioningService"/>.
/// </summary>
/// <remarks>
/// Runs once during startup before the server begins accepting requests.
/// Failures are logged and do not crash the application, allowing operators
/// to handle provisioning manually if needed.
/// </remarks>
internal sealed partial class TenantProvisioningHostedService : IHostedService
{
    private readonly ITenantProvisioningService _provisioningService;
    private readonly string _defaultTenantId;
    private readonly Scope[] _defaultScopes;
    private readonly ILogger<TenantProvisioningHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="TenantProvisioningHostedService"/>.
    /// </summary>
    /// <param name="provisioningService">The provisioning service to invoke.</param>
    /// <param name="defaultTenantId">The default tenant to provision on startup.</param>
    /// <param name="defaultScopes">Additional scopes to seed for the default tenant.</param>
    /// <param name="logger">The logger.</param>
    public TenantProvisioningHostedService(
        ITenantProvisioningService provisioningService,
        string defaultTenantId,
        Scope[] defaultScopes,
        ILogger<TenantProvisioningHostedService> logger)
    {
        _provisioningService = provisioningService;
        _defaultTenantId = defaultTenantId;
        _defaultScopes = defaultScopes;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LogProvisioningDefaultTenantTenantid(_defaultTenantId);

        try
        {
            var success = await _provisioningService
                .ProvisionAsync(_defaultTenantId, _defaultScopes, cancellationToken)
                .ConfigureAwait(false);

            if (success)
            {
                LogDefaultTenantTenantidProvisionedSuccessfully(_defaultTenantId);
            }
            else
            {
                LogDefaultTenantTenantidProvisioningReturnedFalse(_defaultTenantId);
            }
        }
        catch (Exception ex)
        {
            // Log and continue — provisioning failures must not prevent the server
            // from starting so operators can diagnose and re-trigger provisioning.
            LogFailedToProvisionDefaultTenantTenantid(_defaultTenantId, ex);
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "Provisioning default tenant '{TenantId}'...")]
    partial void LogProvisioningDefaultTenantTenantid(string tenantId);

    [LoggerMessage(LogLevel.Information, "Default tenant '{TenantId}' provisioned successfully.")]
    partial void LogDefaultTenantTenantidProvisionedSuccessfully(string tenantId);

    [LoggerMessage(LogLevel.Warning, "Default tenant '{TenantId}' provisioning returned false.")]
    partial void LogDefaultTenantTenantidProvisioningReturnedFalse(string tenantId);

    [LoggerMessage(LogLevel.Error, "Failed to provision default tenant '{TenantId}'.")]
    partial void LogFailedToProvisionDefaultTenantTenantid(string tenantId, Exception exception);
}


