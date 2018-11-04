using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using Shared;

    public class GetResourceSetResult : BaseResponse
    {
        public ResourceSetResponse Content { get; set; }
    }
}
