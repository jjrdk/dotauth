namespace SimpleIdentityServer.Manager.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public class GetScopeResult : BaseResponse
    {
        public ScopeResponse Content { get; set; }
    }
}
