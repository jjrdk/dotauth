using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using Core.Common;

    public class AddResourceSetResult : BaseResponse
    {
        public AddResourceSetResponse Content { get; set; }
    }
}
