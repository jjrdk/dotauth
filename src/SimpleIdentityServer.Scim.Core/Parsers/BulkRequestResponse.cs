namespace SimpleIdentityServer.Scim.Core.Parsers
{
    using Shared.DTOs;
    using Shared.Models;

    public class BulkRequestResponse
    {
        public BulkResult BulkResult { get; set; }
        public ScimErrorResponse ErrorResponse { get; set; }
        public bool IsParsed { get; set; }
    }
}