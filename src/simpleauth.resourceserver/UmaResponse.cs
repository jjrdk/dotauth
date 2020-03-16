namespace SimpleAuth.ResourceServer
{
    using System;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.DTOs;

    /// <summary>
    /// Defines the UMA response.
    /// </summary>
    public class UmaResponse : IActionResult
    {
        private readonly Uri _umaAuthority;
        private readonly PermissionRequest[] _permissionRequest;
        private readonly IUmaPermissionClient _permissionClient;
        private readonly string _realm;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmaResponse"/> class.
        /// </summary>
        /// <param name="umaAuthority"></param>
        /// <param name="permissionRequest"></param>
        /// <param name="permissionClient">The client to use to request permission token.</param>
        /// <param name="realm">The application realm.</param>
        public UmaResponse(Uri umaAuthority, IUmaPermissionClient permissionClient, string realm = null, params PermissionRequest[] permissionRequest)
        {
            _umaAuthority = umaAuthority ?? throw new ArgumentNullException(nameof(umaAuthority));
            _permissionRequest = permissionRequest ?? throw new ArgumentNullException(nameof(permissionRequest));
            if (_permissionRequest.Length == 0)
            {
                throw new ArgumentException("Must provide at least one permission request.", nameof(permissionRequest));
            }
            _permissionClient = permissionClient ?? throw new ArgumentNullException(nameof(permissionClient));
            _realm = realm;
        }

        /// <inheritdoc />
        public async Task ExecuteResultAsync(ActionContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorization))
            {
                throw new InvalidOperationException("Cannot request ticket without access token.");
            }

            if (!AuthenticationHeaderValue.TryParse(authorization[0], out var headerValue)
                || !string.Equals("Bearer", headerValue.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot request ticket without bearer token.");
            }

            var ticket = _permissionRequest.Length == 1
                ? await _permissionClient.RequestPermission(
                        headerValue.Parameter,
                        _permissionRequest[0],
                        CancellationToken.None)
                    .ConfigureAwait(false)
                : await _permissionClient.RequestPermissions(
                        headerValue.Parameter,
                        CancellationToken.None,
                        _permissionRequest)
                    .ConfigureAwait(false);
            context.HttpContext.Response.ConfigureResponse(ticket, _umaAuthority, _realm);
        }
    }
}