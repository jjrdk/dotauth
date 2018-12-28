namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Uma.Shared.DTOs;

    public class SearchAuthPoliciesResult : BaseResponse
    {
        public SearchAuthPoliciesResponse Content { get; set; }
    }
}
