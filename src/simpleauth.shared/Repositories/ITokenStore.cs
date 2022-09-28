namespace SimpleAuth.Shared.Repositories;

using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using SimpleAuth.Shared.Models;

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
    /// <param name="getRefreshToken">The get refresh token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<GrantedToken?> GetRefreshToken(string getRefreshToken, CancellationToken cancellationToken);

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
}