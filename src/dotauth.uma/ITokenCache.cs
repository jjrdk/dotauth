namespace DotAuth.Uma;

using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Requests;
using DotAuth.Shared.Responses;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Defines the token cache interface
/// </summary>
public interface ITokenCache
{
    /// <summary>
    /// Returns whether an access token for the given scopes already exists in the cache.
    /// </summary>
    /// <param name="scopes">The desired scope access.</param>
    /// <returns><c>true</c> is the token exists in the cache, otherwise <c>false</c>.</returns>
    public bool HasAccessToken(params string[] scopes);

    /// <summary>
    /// Gets the token as an async operation.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation.</param>
    /// <param name="scopes">The scopes to request access to.</param>
    /// <returns>The <see cref="GrantedTokenResponse"/> as a <see cref="Task{TResult}"/>.</returns>
    ValueTask<GrantedTokenResponse?> GetToken(CancellationToken cancellationToken = default, params string[] scopes);

    /// <summary>
    /// Gets the token as an async operation.
    /// </summary>
    /// <param name="idToken">The id token as a JWT string for the user requesting access.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation.</param>
    /// <param name="permissions">The resource sets to include in the token request.</param>
    /// <returns>The <see cref="GrantedTokenResponse"/> as a <see cref="Task{TResult}"/>.</returns>
    ValueTask<GrantedTokenResponse?> GetUmaToken(string idToken, CancellationToken cancellationToken = default, params PermissionRequest[] permissions);

    /// <summary>
    /// Gets the JSON Web Key set.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the operation.</param>
    /// <returns>The public <see cref="JsonWebKeySet"/> of the server as a <see cref="Task{TResult}"/>.</returns>
    ValueTask<JsonWebKeySet> GetJwks(CancellationToken cancellationToken = default);
}
