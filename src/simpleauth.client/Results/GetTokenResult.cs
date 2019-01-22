namespace SimpleAuth.Client.Results
{
    using Shared.Responses;

    public class GetTokenResult : BaseSidResult
    {
        public GrantedTokenResponse Content { get; set; }
    }
}
