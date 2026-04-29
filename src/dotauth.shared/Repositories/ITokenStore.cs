namespace DotAuth.Shared.Repositories;

using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using DotAuth.Shared.Models;

/// <summary>
/// Defines the token store interface.
/// </summary>
public interface ITokenStore
{
    /// <summary>
    /// Try to get a valid access token.
    /// </summary>
    /// <param name="scopes"></param>
    /// <param name="clientId"></param>
    /// <param name="idTokenJwsPayload"></param>
    /// <param name="userInfoJwsPayload"></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the async operation.</param>
    /// <returns></returns>
    Task<GrantedToken?> GetToken(
        string scopes,
        string clientId,
        JwtPayload? idTokenJwsPayload = null,
        JwtPayload? userInfoJwsPayload = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the refresh token.
    /// </summary>
    /// <param name="refreshToken">The get refresh token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<GrantedToken?> GetRefreshToken(string refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the access token.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<GrantedToken?> GetAccessToken(string accessToken, CancellationToken cancellationToken);

    /// <summary>
    /// Adds the token.
    /// </summary>
    /// <param name="grantedToken">The granted token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> AddToken(GrantedToken grantedToken, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> RemoveRefreshToken(string refreshToken, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the access token.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<bool> RemoveAccessToken(string accessToken, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically consumes (gets + removes) a refresh token.
    /// Implementations should return the consumed <see cref="GrantedToken"/> when the
    /// refresh token existed and was removed; otherwise return null.
    /// This is intended to avoid race conditions where concurrent refresh exchanges
    /// could both observe and remove the same refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The consumed granted token, or null if it did not exist.</returns>
    Task<GrantedToken?> ConsumeRefreshToken(string refreshToken, CancellationToken cancellationToken);
}
