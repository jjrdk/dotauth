namespace SimpleIdentityServer.Manager.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;

    public class AddClientResult : BaseResponse
    {
        public Client Content { get; set; }
    }
}
