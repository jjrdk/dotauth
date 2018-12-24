using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public class GetAllClaimsResult : BaseResponse
    {
        public IEnumerable<ClaimResponse> Content { get; set; }
    }
}
