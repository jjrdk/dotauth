namespace SimpleIdentityServer.Shared
{
    using System.Net;
    using Responses;

    public class BaseResponse
    {
        public HttpStatusCode HttpStatus { get; set; }
        public bool ContainsError { get; set; }
        public ErrorResponse Error { get; set; }
    }
}