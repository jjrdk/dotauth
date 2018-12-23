using System.Collections.Generic;

namespace SimpleIdentityServer.Manager.Client.Results
{
    using Shared;
    using Shared.Responses;

    public class GetAllScopesResult : BaseResponse
    {
        public IEnumerable<ScopeResponse> Content { get; set; }
    }
}
