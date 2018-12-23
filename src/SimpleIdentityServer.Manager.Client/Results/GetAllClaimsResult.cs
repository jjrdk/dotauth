using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Client.Results
{
    using Shared;
    using Shared.Responses;

    public class GetAllClaimsResult : BaseResponse
    {
        public IEnumerable<ClaimResponse> Content { get; set; }
    }
}
