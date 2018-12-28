namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Uma.Shared.DTOs;

    public class AddResourceSetResult : BaseResponse
    {
        public AddResourceSetResponse Content { get; set; }
    }
}
