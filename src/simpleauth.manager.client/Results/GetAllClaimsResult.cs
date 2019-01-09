namespace SimpleAuth.Manager.Client.Results
{
    using System.Collections.Generic;
    using Shared;
    using Shared.Responses;

    public class GetAllClaimsResult : BaseResponse
    {
        public IEnumerable<ClaimResponse> Content { get; set; }
    }
}
