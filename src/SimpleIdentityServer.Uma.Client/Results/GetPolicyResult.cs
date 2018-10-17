using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using Core.Common;

    public class GetPolicyResult : BaseResponse
    {
        public PolicyResponse Content { get; set; }
    }
}
