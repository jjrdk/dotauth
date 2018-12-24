using System.Collections.Generic;

namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;

    public class GetPoliciesResult : BaseResponse
    {
        public IEnumerable<string> Content { get; set; }
    }
}
