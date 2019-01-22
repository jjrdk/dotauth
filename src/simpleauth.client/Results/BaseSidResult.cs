namespace SimpleAuth.Client.Results
{
    using System.Net;
    using Shared.Responses;

    public class BaseSidResult
    {
        public bool ContainsError { get; set; }
        public ErrorResponseWithState Error { get; set;}
        public HttpStatusCode Status { get; set; }
    }
}
