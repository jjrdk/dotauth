namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using SimpleIdentityServer.Core.Common.DTOs;
    using SimpleIdentityServer.Core.Common.Models;

    public class BulkRequestResponse
    {
        public BulkResult BulkResult { get; set; }
        public ScimErrorResponse ErrorResponse { get; set; }
        public bool IsParsed { get; set; }
    }
}