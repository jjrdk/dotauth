namespace SimpleAuth.Uma.Client.Results
{
    using Shared.DTOs;
    using SimpleAuth.Shared;

    public class SearchAuthPoliciesResult : BaseResponse
    {
        public SearchAuthPoliciesResponse Content { get; set; }
    }
}
