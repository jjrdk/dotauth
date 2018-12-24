using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public class GetAllScopesResult : BaseResponse
    {
        public IEnumerable<ScopeResponse> Content { get; set; }
    }
}
