namespace SimpleAuth.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
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
        private readonly IClientStore _clientStore;
        private readonly DeviceAuthorizationActions _actions;
        private readonly ILogger<DeviceController> _logger;

        public DeviceAuthorizationController(IClientStore clientStore, DeviceAuthorizationActions actions, ILogger<DeviceController> logger)
        {
            _clientStore = clientStore;
            _actions = actions;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> RequestAuthorization([FromForm] TokenRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.client_id)
                || await _clientStore.GetById(request.client_id, cancellationToken).ConfigureAwait(false) == null)
            {
                _logger.LogError("Client not found: {0}", request.client_id);
                return BadRequest();
            }

            var scopeArray = string.IsNullOrWhiteSpace(request.scope) ? Array.Empty<string>() : request.scope.Split(' ', StringSplitOptions.TrimEntries).ToArray();
            var response = await _actions.StartDeviceAuthorizationRequest(request.client_id, Request.GetAbsoluteUri(), scopeArray).ConfigureAwait(false);

            return response switch
            {
                Option<DeviceAuthorizationRequest>.Result r => Ok(r.Item),
                Option<DeviceAuthorizationRequest>.Error e => new ObjectResult(e.Details) { StatusCode = (int)HttpStatusCode.InternalServerError },
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}