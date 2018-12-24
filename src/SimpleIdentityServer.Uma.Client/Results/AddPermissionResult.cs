using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;

    public class AddPermissionResult : BaseResponse
    {
        public AddPermissionResponse Content { get; set; }
    }
}
