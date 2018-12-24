namespace SimpleIdentityServer.Manager.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public class PagedResult<T> : BaseResponse
    {
        public PagedResponse<T> Content { get; set; }
    }
}
