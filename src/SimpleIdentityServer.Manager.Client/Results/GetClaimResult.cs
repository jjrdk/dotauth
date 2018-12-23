namespace SimpleIdentityServer.Manager.Client.Results
{
    using Shared;
    using Shared.Responses;

    public class GetClaimResult : BaseResponse
    {
        public ClaimResponse Content { get; set; }
    }
}
