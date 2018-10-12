using SimpleIdentityServer.Uma.Common.DTOs;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using Core.Common;

    public class AddPolicyResult : BaseResponse
    {
        public AddPolicyResponse Content { get; set; }
    }
}