namespace SimpleAuth.ResourceServer
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using SimpleAuth.Client;
    using SimpleAuth.Shared.DTOs;

    /// <summary>
    /// Defines the UMA response.
    /// </summary>
    public class UmaResponse : IActionResult
    {
        private readonly Uri _umaAuthority;
        private readonly PermissionRequest _permissionRequest;
        private readonly IUmaPermissionClient _permissionClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmaResponse"/> class.
        /// </summary>
        /// <param name="umaAuthority"></param>
        /// <param name="permissionRequest"></param>
        /// <param name="permissionClient">The client to use to request permission token.</param>
        public UmaResponse(Uri umaAuthority, PermissionRequest permissionRequest, IUmaPermissionClient permissionClient)
        {
            _umaAuthority = umaAuthority;
            _permissionRequest = permissionRequest;
            _permissionClient = permissionClient;
        }

        /// <inheritdoc />
        public Task ExecuteResultAsync(ActionContext context)
        {
            return Task.CompletedTask;
        }
    }
}