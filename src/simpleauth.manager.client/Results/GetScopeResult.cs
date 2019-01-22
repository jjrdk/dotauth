namespace SimpleAuth.Manager.Client.Results
{
    using Shared;
    using Shared.Responses;

    public class GetScopeResult : BaseResponse
    {
        public ScopeResponse Content { get; set; }
    }
}
