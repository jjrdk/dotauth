namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Uma.Shared.DTOs;

    public class GetResourceSetResult : BaseResponse
    {
        public ResourceSetResponse Content { get; set; }
    }
}
