namespace SimpleAuth.Client.Results
{
    using Shared.Responses;

    public class GetIntrospectionResult : BaseSidResult
    {
        public IntrospectionResponse Content { get; set; }
    }
}
