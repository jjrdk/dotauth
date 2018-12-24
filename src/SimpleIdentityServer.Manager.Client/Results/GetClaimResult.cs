namespace SimpleIdentityServer.Manager.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public class GetClaimResult : BaseResponse
    {
        public ClaimResponse Content { get; set; }
    }
}
