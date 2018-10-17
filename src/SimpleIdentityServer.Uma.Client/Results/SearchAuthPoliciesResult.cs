using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using Core.Common;

    public class SearchAuthPoliciesResult : BaseResponse
    {
        public SearchAuthPoliciesResponse Content { get; set; }
    }
}
