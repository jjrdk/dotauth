using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using Core.Common;

    public class UpdateResourceSetResult : BaseResponse
    {
        public UpdateResourceSetResponse Content { get; set; }
    }
}
