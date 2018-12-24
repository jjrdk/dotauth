using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;

    public class UpdateResourceSetResult : BaseResponse
    {
        public UpdateResourceSetResponse Content { get; set; }
    }
}
