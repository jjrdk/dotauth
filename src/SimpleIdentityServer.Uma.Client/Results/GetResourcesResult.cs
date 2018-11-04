using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using Shared;

    public class GetResourcesResult : BaseResponse
    {
        public IEnumerable<string> Content { get; set; }
    }
}