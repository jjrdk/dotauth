namespace SimpleAuth.Manager.Client.Results
{
    using System.Collections.Generic;
    using Shared;
    using Shared.Responses;

    public class GetAllClientResult : BaseResponse
    {
        public IEnumerable<ClientResponse> Content { get; set; }
    }
}
