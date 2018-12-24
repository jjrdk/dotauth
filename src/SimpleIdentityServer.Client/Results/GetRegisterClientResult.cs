namespace SimpleIdentityServer.Client.Results
{
    using SimpleAuth.Shared.Models;

    public class GetRegisterClientResult : BaseSidResult
    {
        public Client Content { get; set; }
    }
}
