namespace DotAuth.Extensions;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Services;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Telemetry;

internal static class ResourceOwnerAuthenticateHelper
{
    public static Task<ResourceOwner?> Authenticate(
        this IAuthenticateResourceOwnerService[] services,
        string login,
        string password,
        CancellationToken cancellationToken,
        params string[] exceptedAmrValues)
    {
        var currentAmrs = services.Select(s => s.Amr).ToArray();
        var amr = currentAmrs.GetAmr(exceptedAmrValues);
        var resolvedAmr = amr is Option<string>.Result amrResult ? amrResult.Item : null;
        using var activity = DotAuthTelemetry.StartInternalActivity(DotAuthTelemetry.ActivityNames.ResourceOwnerAuthenticate);
        activity?.SetTag(DotAuthTelemetry.TagKeys.Amr, DotAuthTelemetry.Normalize(resolvedAmr));
        activity?.SetTag(DotAuthTelemetry.TagKeys.ResourceOwner2FaRequired, false);
        if (amr is not Option<string>.Result result)
        {
            activity?.SetTag(DotAuthTelemetry.TagKeys.ResourceOwnerFound, false);
            activity?.SetTag(DotAuthTelemetry.TagKeys.Status, "no_matching_amr");
            activity?.SetStatus(ActivityStatusCode.Error);
            DotAuthTelemetry.RecordResourceOwnerAuthenticationFailure(resolvedAmr);
            return Task.FromResult<ResourceOwner?>(null);
        }

        var service = services.Single(s => s.Amr == result.Item);
        return AuthenticateWithTelemetry(service, login, password, cancellationToken, activity);
    }

    public static IEnumerable<string> GetAmrs(this IEnumerable<IAuthenticateResourceOwnerService> services)
    {
        return services.Select(s => s.Amr);
    }

    /// <summary>
    /// Executes resource-owner authentication while enriching the active telemetry span.
    /// </summary>
    /// <param name="service">The selected authentication service.</param>
    /// <param name="login">The login identifier.</param>
    /// <param name="password">The supplied password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="activity">The active telemetry activity.</param>
    /// <returns>The authenticated resource owner when authentication succeeds.</returns>
    private static async Task<ResourceOwner?> AuthenticateWithTelemetry(
        IAuthenticateResourceOwnerService service,
        string login,
        string password,
        CancellationToken cancellationToken,
        Activity? activity)
    {
        var resourceOwner = await service.AuthenticateResourceOwner(login, password, cancellationToken).ConfigureAwait(false);
        var found = resourceOwner is not null;
        activity?.SetTag(DotAuthTelemetry.TagKeys.ResourceOwnerFound, found);
        if (!found)
        {
            activity?.SetTag(DotAuthTelemetry.TagKeys.Status, "authentication_failed");
            activity?.SetStatus(ActivityStatusCode.Error);
            DotAuthTelemetry.RecordResourceOwnerAuthenticationFailure(service.Amr);
            return null;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        DotAuthTelemetry.RecordResourceOwnerAuthenticationSuccess(service.Amr);
        return resourceOwner;
    }
}
