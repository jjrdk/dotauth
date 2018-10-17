namespace SimpleIdentityServer.Scim.Core.Factories
{
    using System.Net;
    using SimpleIdentityServer.Core.Common.DTOs;

    public interface IErrorResponseFactory
    {
        ScimErrorResponse CreateError(string detail, HttpStatusCode status);
        ScimErrorResponse CreateError(string detail, HttpStatusCode status, string scimType);
    }
}