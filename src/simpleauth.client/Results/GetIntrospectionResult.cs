namespace SimpleIdentityServer.Client.Results
{
    using SimpleAuth.Shared.Responses;

    public class GetIntrospectionResult : BaseSidResult
    {
        public IntrospectionResponse Content { get; set; }
    }
}
