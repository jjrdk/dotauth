namespace SimpleAuth.Controllers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using SimpleAuth.Api.Device;
    using SimpleAuth.Common;
    using SimpleAuth.Filters;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Repositories;
    using SimpleAuth.Shared.Requests;

    [Route(CoreConstants.EndPoints.DeviceAuthorization)]
    [ThrottleFilter]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class DeviceAuthorizationController : ControllerBase
    {
        private readonly DeviceAuthorizationActions _actions;

        public DeviceAuthorizationController(
            IClientStore clientStore,
            IDeviceAuthorizationStore deviceAuthorizationStore,
            ILogger<DeviceController> logger)
        {
            _actions = new DeviceAuthorizationActions(deviceAuthorizationStore, clientStore, logger);
        }

        [HttpPost]
        public async Task<IActionResult> RequestAuthorization([FromForm] TokenRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.client_id))
            {
                return BadRequest();
            }
            var scopeArray = string.IsNullOrWhiteSpace(request.scope) ? Array.Empty<string>() : request.scope.Split(' ', StringSplitOptions.TrimEntries).ToArray();
            var response = await _actions.StartDeviceAuthorizationRequest(
                    request.client_id,
                    Request.GetAbsoluteUri(),
                    scopeArray,
                    cancellationToken)
                .ConfigureAwait(false);

            return response switch
            {
                Option<DeviceAuthorizationData>.Result r => Ok(r.Item.Response),
                Option<DeviceAuthorizationData>.Error e => new ObjectResult(e.Details) { StatusCode = (int)e.Details.Status },
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}