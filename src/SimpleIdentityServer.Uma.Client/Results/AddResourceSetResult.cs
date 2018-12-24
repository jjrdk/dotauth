using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;

    public class AddResourceSetResult : BaseResponse
    {
        public AddResourceSetResponse Content { get; set; }
    }
}
