using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Client.Results
{
    using Shared;
    using Shared.Responses;

    public class GetAllClientResult : BaseResponse
    {
        public IEnumerable<ClientResponse> Content { get; set; }
    }
}
