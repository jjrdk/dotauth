namespace SimpleAuth.Client
{
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Requests;
    using SimpleAuth.Shared.Responses;

    /// <summary>
    /// Defines the resource client interface.
    /// </summary>
    public interface IResourceClient
    {
        /// <summary>
        /// Updates the resource.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// token
        /// </exception>
        Task<GenericResponse<UpdateResourceSetResponse>> UpdateResource(
            ResourceSet request,
            string token);

        /// <summary>
        /// Adds the resource.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// token
        /// </exception>
        Task<GenericResponse<AddResourceSetResponse>> AddResource(ResourceSet request, string token);

        /// <summary>
        /// Deletes the resource.
        /// </summary>
        /// <param name="resourceSetId">The resource set identifier.</param>
        /// <param name="authorizationHeaderValue">The authorization header value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// resourceSetId
        /// or
        /// authorizationHeaderValue
        /// </exception>
        Task<GenericResponse<object>> DeleteResource(string resourceSetId, string authorizationHeaderValue);

        /// <summary>
        /// Gets all resources.
        /// </summary>
        /// <param name="authorizationHeaderValue">The authorization header value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">authorizationHeaderValue</exception>
        Task<GenericResponse<ResourceSet[]>> GetAllResources(string authorizationHeaderValue);

        /// <summary>
        /// Gets the resource.
        /// </summary>
        /// <param name="resourceSetId">The resource set identifier.</param>
        /// <param name="authorizationHeaderValue">The authorization header value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// resourceSetId
        /// or
        /// authorizationHeaderValue
        /// </exception>
        Task<GenericResponse<ResourceSet>> GetResource(
            string resourceSetId,
            string authorizationHeaderValue);

        /// <summary>
        /// Searches the resources.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="authorizationHeaderValue">The authorization header value.</param>
        /// <returns></returns>
        Task<GenericResponse<GenericResult<ResourceSet>>> SearchResources(
            SearchResourceSet parameter,
            string authorizationHeaderValue = null);
    }
}