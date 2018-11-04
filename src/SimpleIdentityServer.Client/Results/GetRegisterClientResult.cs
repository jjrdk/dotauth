namespace SimpleIdentityServer.Client.Results
{
    using Shared.Responses;

    public class GetRegisterClientResult : BaseSidResult
    {
        public ClientRegistrationResponse Content { get; set; }
    }
}
