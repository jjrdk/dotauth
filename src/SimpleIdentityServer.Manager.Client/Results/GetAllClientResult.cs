using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public class GetAllClientResult : BaseResponse
    {
        public IEnumerable<ClientResponse> Content { get; set; }
    }
}
