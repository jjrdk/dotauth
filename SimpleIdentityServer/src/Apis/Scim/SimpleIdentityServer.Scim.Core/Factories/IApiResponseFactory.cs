namespace SimpleIdentityServer.Scim.Core.Factories
{
    using System.Net;
    using Results;
    using SimpleIdentityServer.Core.Common.DTOs;

    public interface IApiResponseFactory
    {
        ApiActionResult CreateEmptyResult(
            HttpStatusCode status);

        ApiActionResult CreateEmptyResult(
            HttpStatusCode status,
            string version,
            string id);

        ApiActionResult CreateResultWithContent(
            HttpStatusCode status,
            object content);

        ApiActionResult CreateResultWithContent(
            HttpStatusCode status,
            object content,
            string location);

        ApiActionResult CreateResultWithContent(
            HttpStatusCode status,
            object content,
            string location,
            string version,
            string id);

        ApiActionResult CreateError(
            HttpStatusCode statusCode,
            string content);

        ApiActionResult CreateError(
            HttpStatusCode status,
            ScimErrorResponse error);
    }
}