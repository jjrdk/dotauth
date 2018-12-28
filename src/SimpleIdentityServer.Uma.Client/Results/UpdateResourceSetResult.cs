namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Uma.Shared.DTOs;

    public class UpdateResourceSetResult : BaseResponse
    {
        public UpdateResourceSetResponse Content { get; set; }
    }
}
