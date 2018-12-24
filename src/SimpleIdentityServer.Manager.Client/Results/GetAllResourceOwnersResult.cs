using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public class GetAllResourceOwnersResult : BaseResponse
    {
        public IEnumerable<ResourceOwnerResponse> Content { get; set; }
    }
}
