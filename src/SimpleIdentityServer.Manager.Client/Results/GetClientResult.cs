namespace SimpleIdentityServer.Manager.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    public sealed class GetClientResult : BaseResponse
    {
        public Client Content { get; set; }
    }
}
