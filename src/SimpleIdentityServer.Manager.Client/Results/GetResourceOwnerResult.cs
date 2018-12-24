namespace SimpleIdentityServer.Manager.Client.Results
{
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Responses;

    public class GetResourceOwnerResult : BaseResponse
    {
        public ResourceOwnerResponse Content { get; set; }
    }
}
