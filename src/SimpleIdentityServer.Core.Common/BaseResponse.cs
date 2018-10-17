namespace SimpleIdentityServer.Core.Common
{
    using System.Net;
    using SimpleIdentityServer.Common.Dtos.Responses;

    public class BaseResponse
    {
        public HttpStatusCode HttpStatus { get; set; }
        public bool ContainsError { get; set; }
        public ErrorResponse Error { get; set; }
    }
}