namespace DotAuth.Client;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;

/// <summary>
/// Defines the resource client interface.
/// </summary>
public interface IUmaResourceSetClient
{
    /// <summary>
    /// Approves the ticket by the token owner.
    /// </summary>
    /// <param name="ticketId">The id of the ticket to approve.</param>
    /// <param name="accessToken">The access token of the approving user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>A list of residual ticket requests as an async operation.</returns>
    Task<Option<IReadOnlyList<Ticket>>> ApproveTicket(string ticketId, string accessToken, CancellationToken cancellationToken = default);

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
    Task<Option<UpdateResourceSetResponse>> UpdateResourceSet(
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
    Task<Option<AddResourceSetResponse>> AddResourceSet(ResourceSet request, string token, CancellationToken cancellationToken = default);

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
    Task<Option> DeleteResource(string resourceSetId, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all resources.
    /// </summary>
    /// <param name="token">The authorization header value.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">authorizationHeaderValue</exception>
    Task<Option<string[]>> GetAllOwnResourceSets(string token, CancellationToken cancellationToken = default);

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
    Task<Option<ResourceSet>> GetResourceSet(
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
    Task<Option<PagedResult<ResourceSetDescription>>> SearchResources(
        SearchResourceSet parameter,
        string? token = null,
        CancellationToken cancellationToken = default);
}