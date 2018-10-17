using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using Core.Common;

    public class SearchResourceSetResult : BaseResponse
    {
        public SearchResourceSetResponse Content { get; set; }
    }
}
