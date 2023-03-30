namespace DotAuth.Client;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;

/// <summary>
/// Defines the dynamic registration client interface.
/// </summary>
public interface IDynamicRegistrationClient
{
    /// <summary>
    /// Sends a request to the authorization server to register a client.
    /// </summary>
    /// <param name="accessToken">An access token collected in an out of band token flow.</param>
    /// <param name="request">The definition content for the client to register.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>A client definition as a <see cref="DynamicClientRegistrationResponse"/>.</returns>
    Task<Option<DynamicClientRegistrationResponse>> Register(
        string accessToken,
        DynamicClientRegistrationRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Alters an existing client registration.
    /// </summary>
    /// <param name="accessToken">An access token collected in an out of band token flow.</param>
    /// <param name="clientId">The id of the client to modify.</param>
    /// <param name="request">The content modifications to the client.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns>An updated client definition as a <see cref="DynamicClientRegistrationResponse"/>.</returns>
    Task<Option<DynamicClientRegistrationResponse>> Modify(
        string accessToken,
        string clientId,
        DynamicClientRegistrationRequest request,
        CancellationToken cancellationToken);
}