namespace SimpleAuth.Uma.Client.Results
{
    using Shared.DTOs;
    using SimpleAuth.Shared;

    public class SearchResourceSetResult : BaseResponse
    {
        public SearchResourceSetResponse Content { get; set; }
    }
}
