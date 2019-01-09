namespace SimpleAuth.Uma.Client.Results
{
    using Shared.DTOs;
    using SimpleAuth.Shared;

    public class AddPolicyResult : BaseResponse
    {
        public AddPolicyResponse Content { get; set; }
    }
}