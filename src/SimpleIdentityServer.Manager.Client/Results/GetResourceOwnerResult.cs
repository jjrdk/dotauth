namespace SimpleIdentityServer.Manager.Client.Results
{
    using Shared;
    using Shared.Responses;

    public class GetResourceOwnerResult : BaseResponse
    {
        public ResourceOwnerResponse Content { get; set; }
    }
}
