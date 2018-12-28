namespace SimpleAuth.Uma.Client.Results
{
    using Shared.DTOs;
    using SimpleAuth.Shared;

    public class GetResourceSetResult : BaseResponse
    {
        public ResourceSetResponse Content { get; set; }
    }
}
