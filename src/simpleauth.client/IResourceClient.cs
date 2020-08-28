namespace SimpleAuth.Client
{
    using System;
    using System.Threading;
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// token
        /// </exception>
        Task<GenericResponse<UpdateResourceSetResponse>> UpdateResource(
            ResourceSet request,
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds the resource.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// request
        /// or
        /// token
        /// </exception>
        Task<GenericResponse<AddResourceSetResponse>> AddResource(ResourceSet request, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the resource.
        /// </summary>
        /// <param name="resourceSetId">The resource set identifier.</param>
        /// <param name="token">The authorization header value.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// resourceSetId
        /// or
        /// authorizationHeaderValue
        /// </exception>
        Task<GenericResponse<object>> DeleteResource(string resourceSetId, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all resources.
        /// </summary>
        /// <param name="token">The authorization header value.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">authorizationHeaderValue</exception>
        Task<GenericResponse<string[]>> GetAllResources(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the resource.
        /// </summary>
        /// <param name="resourceSetId">The resource set identifier.</param>
        /// <param name="token">The authorization header value.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// resourceSetId
        /// or
        /// authorizationHeaderValue
        /// </exception>
        Task<GenericResponse<ResourceSet>> GetResource(
            string resourceSetId,
            string token,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches the resources.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <param name="token">The authorization header value.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
        /// <returns></returns>
        Task<GenericResponse<PagedResult<ResourceSet>>> SearchResources(
            SearchResourceSet parameter,
            string? token = null,
            CancellationToken cancellationToken = default);
    }
}