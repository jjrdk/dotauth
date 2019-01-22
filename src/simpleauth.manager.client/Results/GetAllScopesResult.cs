namespace SimpleAuth.Manager.Client.Results
{
    using System.Collections.Generic;
    using Shared;
    using Shared.Responses;

    public class GetAllScopesResult : BaseResponse
    {
        public IEnumerable<ScopeResponse> Content { get; set; }
    }
}
