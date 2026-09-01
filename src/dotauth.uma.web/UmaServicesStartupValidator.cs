namespace DotAuth.Uma.Web;

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using DotAuth.Client;

/// <summary>
/// An <see cref="IStartupFilter"/> that validates the presence of required UMA services in the DI container
/// at application startup, producing clear error messages instead of cryptic runtime failures.
/// </summary>
internal sealed class UmaServicesStartupValidator : IStartupFilter
{
    private readonly IServiceProvider _serviceProvider;

    public UmaServicesStartupValidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        ValidateRequiredServices();
        return next;
    }

    private void ValidateRequiredServices()
    {
        if (_serviceProvider.GetService(typeof(DotAuth.Uma.IResourceMap)) is null)
        {
            throw new InvalidOperationException(
                "UMA bearer authentication requires a registered IResourceMap service. " +
                "Call services.AddSingleton<IResourceMap, YourResourceMap>() before calling Build().");
        }

        if (_serviceProvider.GetService(typeof(IUmaPermissionClient)) is null)
        {
            throw new InvalidOperationException(
                "UMA bearer authentication requires a registered IUmaPermissionClient service. " +
                "Call services.AddSingleton<IUmaPermissionClient, YourPermissionClient>() before calling Build().");
        }

        if (_serviceProvider.GetService(typeof(ITokenClient)) is null)
        {
            throw new InvalidOperationException(
                "UMA bearer authentication requires a registered ITokenClient service. " +
                "Call services.AddSingleton<ITokenClient, YourTokenClient>() before calling Build().");
        }
    }
}
