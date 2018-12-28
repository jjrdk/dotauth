namespace SimpleAuth.Manager.Client.Results
{
    using System.Collections.Generic;
    using Shared;
    using Shared.Responses;

    public class GetAllResourceOwnersResult : BaseResponse
    {
        public IEnumerable<ResourceOwnerResponse> Content { get; set; }
    }
}
