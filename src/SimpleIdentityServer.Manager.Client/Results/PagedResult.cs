namespace SimpleIdentityServer.Manager.Client.Results
{
    using Shared;
    using Shared.Responses;

    public class PagedResult<T> : BaseResponse
    {
        public PagedResponse<T> Content { get; set; }
    }
}
