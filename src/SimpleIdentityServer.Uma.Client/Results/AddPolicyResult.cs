namespace SimpleIdentityServer.Uma.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Uma.Shared.DTOs;

    public class AddPolicyResult : BaseResponse
    {
        public AddPolicyResponse Content { get; set; }
    }
}