namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Uma.Shared.DTOs;

    public class SearchResourceSetResult : BaseResponse
    {
        public SearchResourceSetResponse Content { get; set; }
    }
}
