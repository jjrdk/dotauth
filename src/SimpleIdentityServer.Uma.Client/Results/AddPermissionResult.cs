using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using Core.Common;

    public class AddPermissionResult : BaseResponse
    {
        public AddPermissionResponse Content { get; set; }
    }
}
