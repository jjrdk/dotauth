namespace SimpleIdentityServer.Manager.Client.Results
{
    using Shared;
    using Shared.Models;

    public class AddClientResult : BaseResponse
    {
        public Client Content { get; set; }
    }
}
