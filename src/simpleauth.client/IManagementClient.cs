namespace DotAuth.Client;

using System;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Models;
using DotAuth.Shared.Requests;

/// <summary>
/// Defines the management client interface.
/// </summary>
public interface IManagementClient
{
    /// <summary>
    /// Gets the specified <see cref="Client"/> information.
    /// </summary>
    /// <param name="clientId">The client id.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<Client>> GetClient(
        string clientId,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds the passed client.
    /// </summary>
    /// <param name="client">The <see cref="Client"/> to add.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<Client>> AddClient(
        Client client,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified client.
    /// </summary>
    /// <param name="clientId">The client id.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<Client>> DeleteClient(
        string clientId,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing client.
    /// </summary>
    /// <param name="client">The updated <see cref="Client"/>.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<Client>> UpdateClient(
        Client client,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all clients.
    /// </summary>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<Client[]>> GetAllClients(
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for clients.
    /// </summary>
    /// <param name="searchClientParameter">The <see cref="SearchClientsRequest"/>.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<PagedResult<Client>>> SearchClients(
        SearchClientsRequest searchClientParameter,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the specified scope information.
    /// </summary>
    /// <param name="id">The scope id.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <exception cref="ArgumentException">If id is empty or whitespace.</exception>
    /// <returns></returns>
    Task<Option<Scope>> GetScope(
        string id,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds the passed scope.
    /// </summary>
    /// <param name="scope">The <see cref="Scope"/> to add.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<Scope>> AddScope(
        Scope scope,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a client with the passed details.
    /// </summary>
    /// <param name="client">The client definition to register.</param>
    /// <param name="accessToken">The access token for the request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the asynchronous request.</param>
    /// <returns>A response with success or error details.</returns>
    Task<Option<Client>> Register(Client client, string accessToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds the passed resource owner.
    /// </summary>
    /// <param name="resourceOwner">The <see cref="AddResourceOwnerRequest"/>.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<AddResourceOwnerResponse>> AddResourceOwner(
        AddResourceOwnerRequest resourceOwner,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the specified resource owner.
    /// </summary>
    /// <param name="resourceOwnerId"></param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<ResourceOwner>> GetResourceOwner(
        string resourceOwnerId,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified resource owner.
    /// </summary>
    /// <param name="resourceOwnerId"></param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option> DeleteResourceOwner(
        string resourceOwnerId,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the password of the specified resource owner.
    /// </summary>
    /// <param name="updateResourceOwnerPasswordRequest">The <see cref="UpdateResourceOwnerPasswordRequest"/>.</param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option> UpdateResourceOwnerPassword(
        UpdateResourceOwnerPasswordRequest updateResourceOwnerPasswordRequest,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the resource owner claims.
    /// </summary>
    /// <param name="updateResourceOwnerClaimsRequest"></param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option> UpdateResourceOwnerClaims(
        UpdateResourceOwnerClaimsRequest updateResourceOwnerClaimsRequest,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all resource owners.
    /// </summary>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<ResourceOwner[]>> GetAllResourceOwners(
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for resource owners.
    /// </summary>
    /// <param name="searchResourceOwnersRequest">The <see cref="SearchResourceOwnersRequest"/></param>
    /// <param name="authorizationHeaderValue">The authorization token.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<Option<PagedResult<ResourceOwner>>> SearchResourceOwners(
        SearchResourceOwnersRequest searchResourceOwnersRequest,
        string authorizationHeaderValue,
        CancellationToken cancellationToken = default);
}