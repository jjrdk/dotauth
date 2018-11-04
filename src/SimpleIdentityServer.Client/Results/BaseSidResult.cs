using System.Net;

namespace SimpleIdentityServer.Client.Results
{
    using Shared.Responses;

    public class BaseSidResult
    {
        public bool ContainsError { get; set; }
        public ErrorResponseWithState Error { get; set;}
        public HttpStatusCode Status { get; set; }
    }
}
