using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;

    public class SearchResourceSetResult : BaseResponse
    {
        public SearchResourceSetResponse Content { get; set; }
    }
}
