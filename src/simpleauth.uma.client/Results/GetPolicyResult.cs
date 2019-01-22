namespace SimpleAuth.Uma.Client.Results
{
    using Shared.DTOs;
    using SimpleAuth.Shared;

    public class GetPolicyResult : BaseResponse
    {
        public PolicyResponse Content { get; set; }
    }
}
