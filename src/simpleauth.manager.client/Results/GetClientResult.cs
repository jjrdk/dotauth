namespace SimpleAuth.Manager.Client.Results
{
    using Shared;
    using Shared.Models;

    public sealed class GetClientResult : BaseResponse
    {
        public Client Content { get; set; }
    }
}
