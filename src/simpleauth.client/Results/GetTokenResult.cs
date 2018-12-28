namespace SimpleIdentityServer.Client.Results
{
    using SimpleAuth.Shared.Responses;

    public class GetTokenResult : BaseSidResult
    {
        public GrantedTokenResponse Content { get; set; }
    }
}
