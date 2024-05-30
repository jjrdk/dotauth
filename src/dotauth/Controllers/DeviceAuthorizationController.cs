namespace DotAuth.Controllers;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Api.Device;
using DotAuth.Common;
using DotAuth.Extensions;
using DotAuth.Filters;
using DotAuth.Shared;
using DotAuth.Shared.Repositories;
using DotAuth.Shared.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Defines the device authorization controller.
/// </summary>
[Route(CoreConstants.EndPoints.DeviceAuthorization)]
[ThrottleFilter]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class DeviceAuthorizationController : ControllerBase
{
    private readonly DeviceAuthorizationActions _actions;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceAuthorizationController"/> class.
    /// </summary>
    /// <param name="settings">The runtime settings.</param>
    /// <param name="deviceAuthorizationStore">The device authorization store.</param>
    /// <param name="clientStore">The client store.</param>
    /// <param name="logger">The logger.</param>
    public DeviceAuthorizationController(
        RuntimeSettings settings,
        IClientStore clientStore,
        IDeviceAuthorizationStore deviceAuthorizationStore,
        ILogger<DeviceController> logger)
    {
        _actions = new DeviceAuthorizationActions(settings, deviceAuthorizationStore, clientStore, logger);
    }

    /// <summary>
    /// Requests the authorization.
    /// </summary>
    /// <param name="request">The token request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> RequestAuthorization(
        [FromForm] TokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.client_id))
        {
            return BadRequest();
        }

        var scopeArray = string.IsNullOrWhiteSpace(request.scope)
            ? []
            : request.scope.Split(' ', StringSplitOptions.TrimEntries).ToArray();
        var response = await _actions.StartDeviceAuthorizationRequest(
                request.client_id,
                Request.GetAbsoluteUri(),
                scopeArray,
                cancellationToken)
            .ConfigureAwait(false);

        return response switch
        {
            Option<DeviceAuthorizationData>.Result r => Ok(r.Item.Response),
            Option<DeviceAuthorizationData>.Error e => new ObjectResult(e.Details)
            {
                StatusCode = (int) e.Details.Status
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}