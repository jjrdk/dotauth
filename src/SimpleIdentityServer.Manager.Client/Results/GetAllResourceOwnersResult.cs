using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Client.Results
{
    using Shared;
    using Shared.Responses;

    public class GetAllResourceOwnersResult : BaseResponse
    {
        public IEnumerable<ResourceOwnerResponse> Content { get; set; }
    }
}
