namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Uma.Shared.DTOs;

    public class AddPermissionResult : BaseResponse
    {
        public AddPermissionResponse Content { get; set; }
    }
}
