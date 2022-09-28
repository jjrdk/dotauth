namespace SimpleAuth.Controllers;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SimpleAuth.Filters;
using SimpleAuth.Shared;
using SimpleAuth.Shared.Repositories;
using SimpleAuth.ViewModels;

/// <summary>
/// Defines the device controller.
/// </summary>
[Authorize]
[Route(CoreConstants.EndPoints.Device)]
[ThrottleFilter]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class DeviceController : ControllerBase
{
    private readonly IDeviceAuthorizationStore _deviceAuthorizationStore;
    private readonly ILogger<DeviceController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceController"/> class.
    /// </summary>
    /// <param name="deviceAuthorizationStore">The device authorization store.</param>
    /// <param name="logger">The logger.</param>
    public DeviceController(IDeviceAuthorizationStore deviceAuthorizationStore, ILogger<DeviceController> logger)
    {
        _deviceAuthorizationStore = deviceAuthorizationStore;
        _logger = logger;
    }

    /// <summary>
    /// Gets the authorization form.
    /// </summary>
    /// <param name="code">The optional user code.</param>
    /// <returns>The authorization form.</returns>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string code)
    {
        return Ok(new DeviceAuthorizationViewModel { Code = code });
    }

    /// <summary>
    /// Approves the authorization request.
    /// </summary>
    /// <param name="code">The user code.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>The authorized form.</returns>
    [HttpPost]
    public async Task<IActionResult> Approve([FromForm] string code, CancellationToken cancellationToken)
    {
        return await HandleApprove(code, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IActionResult> HandleApprove(string code, CancellationToken cancellationToken)
    {
        var authorization = await _deviceAuthorizationStore.Approve(code, cancellationToken).ConfigureAwait(false);

        if (authorization is Option.Error e)
        {
            _logger.LogError("User code: {0} not found", code);
            return BadRequest(e.Details);
        }

        return Ok(new object());
    }
}