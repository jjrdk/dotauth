namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using SimpleAuth.Shared.DTOs;
    using SimpleAuth.Shared.Models;

    public class BulkRequestResponse
    {
        public BulkResult BulkResult { get; set; }
        public ScimErrorResponse ErrorResponse { get; set; }
        public bool IsParsed { get; set; }
    }
}