namespace SimpleAuth.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Filters;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.ViewModels;

    [Authorize]
    [Route(CoreConstants.EndPoints.Device)]
    [ThrottleFilter]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceAuthorizationStore _deviceAuthorizationStore;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(IDeviceAuthorizationStore deviceAuthorizationStore, ILogger<DeviceController> logger)
        {
            _deviceAuthorizationStore = deviceAuthorizationStore;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string code)
        {
            return Ok(new DeviceAuthorizationViewModel { Code = code });
        }

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
}
